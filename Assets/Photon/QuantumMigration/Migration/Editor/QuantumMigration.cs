using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Quantum;
using Quantum.CodeGen;
using Quantum.Editor;
using Quantum.Migration;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Tools to migrate projects from Quantum 2.1 to Quantum 3.0. Global namespace, to make invoking from
/// command line easier.
/// </summary>
public static class QuantumMigration {
  public const string CodeProjectFolderOption = "-quantumCodeProjectFolder";
  public const string AssetExportRootOption   = "-quantumAssetExportPath";
  public const string CodeUpgradeDefine       = "QUANTUM_ENABLE_MIGRATION_CODE_UPGRADE";

  /// <summary>
  /// Copies *.cs and +.qtn recursively from the Quantum 2.1 quantum_code project folder to the new location in Quantum 3.0 (Assets/QuantumUser/Simulation/)
  /// </summary>
  [MenuItem("Tools/Quantum/Migration/Import Simulation Project", priority = 5000)]
  public static void ImportSimulationProject() {
    string folder;

    if (Application.isBatchMode) {
      folder = GetCommandLineOption(CodeProjectFolderOption);
    } else {
      folder = EditorUtility.OpenFolderPanel("Open the Quantum 2.1 Code Folder (quantum.code)", "../quantum_code", "quantum.code");
      if (string.IsNullOrEmpty(folder)) {
        return;
      }
    }

    // Opening any folder of this path will work: quantum_unity/Assets/Photon/Quantum
    ImportSimulationProject(folder);
  }

  [MenuItem("Tools/Quantum/Migration/Run Initial CodeGen", priority = 5001)]
  public static void RunInitialCodeGen() {
    Log("Running initial code gen");
    QuantumCodeGenQtn.Run(verbose: true, options: new GeneratorOptions() {
      LegacyCodeGenOptions = GeneratorLegacyOptions.AssetRefs | GeneratorLegacyOptions.AssetBaseStubs | GeneratorLegacyOptions.AssetObjectAccessors | GeneratorLegacyOptions.UnderscorePrototypesSuffix | GeneratorLegacyOptions.EntityComponentStubs,
    });
    
    foreach (var group in ValidBuildTargetGroups) {
      if (SetScriptingDefine(group, "QUANTUM_DISABLE_AUTO_CODEGEN", false)) {
        Log("Removed QUANTUM_DISABLE_AUTO_CODEGEN from " + group);
      }
    }
  }

  [MenuItem("Tools/Quantum/Migration/Delete Assembly Definitions (Pre Quantum 2.1-style)", priority = 5002)]
  public static void DeleteAssemblyDefinitions() {
    if (!Application.isBatchMode) {
      var dialogResult = EditorUtility.DisplayDialog("Quantum Warning",
        "Are you sure you want to delete Quantum Assembly Definitions? Select Yes only if your Quantum 2 project did not use Assembly Definitions for Quantum code.",
        "Yes", "No");

      if (!dialogResult) {
        return;
      }
    }

    foreach (var group in ValidBuildTargetGroups) {
      if (SetScriptingDefine(group, "QUANTUM_UNITY", true)) {
        Log("Added QUANTUM_UNITY define to " + group);
      }
    }

    var assemblyDefinitionNames = new[] { "Quantum.Unity", "Quantum.Simulation", "Quantum.Unity.Editor" };

    var paths = AssetDatabase.FindAssets("t:asmdef")
     .Concat(AssetDatabase.FindAssets("t:asmref"))
     .Select(x => AssetDatabase.GUIDToAssetPath(x))
     .Where(x => assemblyDefinitionNames.Contains(Path.GetFileNameWithoutExtension(x)))
     .ToArray();

    foreach (var path in paths) {
      if (AssetDatabase.DeleteAsset(path)) {
        Log($"Deleted {path}");
      }
    }
  }

  [MenuItem("Tools/Quantum/Migration/Check Asset Object Scripts", priority = 5003)]
  public static void CheckAssetObjectScriptsBeingReady() {
    Log("Checking Asset Object Scripts");

    int result = CheckAssetObjectScriptsBeingReadyInternal();
    ConsumeResult(result);
  }

  [MenuItem("Tools/Quantum/Migration/Transfer AssetBase Guids to AssetObjects", priority = 5004)]
  public static void TransferAssetBaseGuidsToAssetObjects() {
    
    const string MovedFromTag = "QuantumMigrationGuidMovedFrom";
    
    var wrapperTypes = TypeCache.GetTypesDerivedFrom<LegacyAssetObjectWrapper>()
     .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition)
     .ToList();

    bool success = true;

    List<(Type wrapperType, MonoScript wrapperScriptType, Type assetType, MonoScript assetWrapperType)> entries = new();
    
    foreach (var wrapperType in wrapperTypes) {
      var errorMessage = CheckTypeScript(wrapperType, out var wrapperScript);
      if (!string.IsNullOrEmpty(errorMessage)) {
        Error(errorMessage);
        success = false;
        continue;
      }

      if (Array.IndexOf(AssetDatabase.GetLabels(wrapperScript), MovedFromTag) >= 0) {
        Log($"Skipping {wrapperType} as it has already been migrated");
        continue;
      }
      
      FieldInfo assetObjectField;
      {
        var tempInstance = (LegacyAssetObjectWrapper)ScriptableObject.CreateInstance(wrapperType);
        try {
          assetObjectField = wrapperType.GetField(tempInstance.AssetObjectPropertyPath, BindingFlags.Instance | BindingFlags.Public);
        } finally {
          Object.DestroyImmediate(tempInstance);
        }
      }

      if (assetObjectField == null) {
        throw new InvalidOperationException($"Could not find asset object field in {wrapperType.FullName}");
      }

      var assetObjectType = assetObjectField.FieldType;

      if (assetObjectType.Assembly == typeof(Quantum.BinaryData).Assembly || assetObjectType.FullName == "Quantum.SimulationConfig") {
        
        // replace existing prototypes with lazy prototypes
        if (assetObjectType == typeof(Quantum.EntityPrototype)) {
          assetObjectType = Type.GetType("Quantum.LazyEntityPrototype, Quantum.Unity") ?? Type.GetType("Quantum.LazyEntityPrototype, Assembly-CSharp") ?? throw new InvalidOperationException("Could not find type Quantum.LazyEntityPrototype");
        }
        
        entries.Add((wrapperType, wrapperScript, assetObjectType, null));
        
      } else {
        errorMessage = CheckTypeScript(assetObjectType, out var assetObjectScript, requireMainAsset: false);
        if (!string.IsNullOrEmpty(errorMessage)) {
          Error(errorMessage);
          success = false;
          continue;
        }

        if (AssetDatabase.IsMainAsset(assetObjectScript)) {
          entries.Add((wrapperType, wrapperScript, assetObjectType, assetObjectScript));  
        } else {
          // not a main asset, needs guid replacement instead
          entries.Add((wrapperType, wrapperScript, assetObjectType, null));
        }
      }
    }

    if (success) {
      List<string> metasToTag = new();
      
      foreach (var (wrapperType, wrapperScript, assetObjectType, assetObjectScript) in entries) {
        if (assetObjectScript == null) {
          // assets need to be fixed up
          var wrapperObjectGuids = AssetDatabase.FindAssets($"t:{wrapperType.Name}");
          
          foreach (var wrapperObjectGuid in wrapperObjectGuids) {
            var wrapperObjectPath = AssetDatabase.GUIDToAssetPath(wrapperObjectGuid);
            
            var importer = AssetImporter.GetAtPath(wrapperObjectPath);
            importer.SaveAndReimport();
            
            var assets = AssetDatabase.LoadAllAssetsAtPath(wrapperObjectPath)
              .Where(x => x.GetType() == wrapperType)
              .ToList();
            
            foreach (var wrapperObject in assets) {
              Log($"Replacing type of {wrapperObject} with {assetObjectType}");
              SetScriptableObjectType(wrapperObject, assetObjectType);
            }
            
            AssetDatabase.SaveAssetIfDirty(new GUID(wrapperObjectGuid));
          }
        } else {
          // asset script guid can be patched instead
          var wrapperScriptPath = AssetDatabase.GetAssetPath(wrapperScript);
          Debug.Assert(!string.IsNullOrEmpty(wrapperScriptPath));

          var assetObjectScriptPath = AssetDatabase.GetAssetPath(assetObjectScript);
          Debug.Assert(!string.IsNullOrEmpty(assetObjectScriptPath));

          var wrapperScriptMetaPath = wrapperScriptPath + ".meta";
          var assetObjectScriptMetaPath = assetObjectScriptPath + ".meta";

          Log($"Swapping {wrapperScriptMetaPath} and {assetObjectScriptMetaPath}");
          var contents = File.ReadAllText(assetObjectScriptMetaPath);
          FileUtil.DeleteFileOrDirectory(assetObjectScriptMetaPath);
          FileUtil.CopyFileOrDirectory(wrapperScriptMetaPath, assetObjectScriptMetaPath);
          File.WriteAllText(wrapperScriptMetaPath, contents);
          
          metasToTag.Add(wrapperScriptPath);
        }
      }
      
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      
      // tag wrappers as moved
      foreach (var wrapperScriptPath in metasToTag) {
        var obj = AssetDatabase.LoadMainAssetAtPath(wrapperScriptPath);
        var labels = AssetDatabase.GetLabels(obj).ToList();
        labels.Add(MovedFromTag);
        AssetDatabase.SetLabels(obj, labels.ToArray());
      }
    }

    ConsumeResult(success);
  }

  [MenuItem("Tools/Quantum/Migration/Upgrade AssetObjects", priority = 5005)]
  public static void RestoreAssetObjectData() {
    string folder;
    if (!Application.isBatchMode) {
      folder = EditorUtility.OpenFolderPanel("Select QuantumUnityDB export folder", "", "");
      if (string.IsNullOrEmpty(folder)) {
        return;
      }
    } else {
      folder = GetCommandLineOption(AssetExportRootOption);
    }

    HashSet<string> usedFiles = new HashSet<string>();

    bool success = true;

    foreach (var assetPath in AssetDatabase.FindAssets($"t:{nameof(AssetObject)}")
              .Select(x => AssetDatabase.GUIDToAssetPath(x))) {

      if (assetPath.StartsWith("Assets/Photon/Quantum/Samples/")) {
        // ignore sample assets
        continue;
      }
      
      var importer = AssetImporter.GetAtPath(assetPath);
      importer.SaveAndReimport();
      
      var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
      var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<AssetObject>();

      foreach (var asset in assets) {
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var unityGuid, out long unityFileId)) {
          Error($"Could not get GUID and local file identifier for {asset}");
          success = false;
          continue;
        }
        
        var serializedAssetPath = $"{folder}/{unityGuid}_{unityFileId}.json";

        if (!File.Exists(serializedAssetPath)) {
          Error($"No asset source found for {assetPath} ({serializedAssetPath})");
          success = false;
          continue;
        }

        usedFiles.Add(Path.GetFileName(serializedAssetPath));
        
        Log($"Importing {serializedAssetPath} to {assetPath}");
        var json = File.ReadAllText(serializedAssetPath);
        JsonUtility.FromJsonOverwrite(File.ReadAllText(serializedAssetPath), asset);

        EditorUtility.SetDirty(asset);
      }
      
      AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assetPath));
    }

    // check if all the exported assets have been used
    var existingFiles = Directory.GetFiles(folder, "*.json")
     .Select(Path.GetFileName)
     .Except(usedFiles)
     .ToList();

    foreach (var file in existingFiles) {
      Error($"Exported file was not used: {file}");
      success = false;
    }
    
    
    
    AssetDatabase.SaveAssets();
    ConsumeResult(success);
  }

  [MenuItem("Tools/Quantum/Migration/Enable AssetObject Postprocessor", priority = 5006)]
  public static void EnableAssetObjectPostprocessor() {
    foreach (var group in ValidBuildTargetGroups) {
      SetScriptingDefine(group, "QUANTUM_DISABLE_ASSET_OBJECT_POSTPROCESSOR", false);
    }
  }

  [MenuItem("Tools/Quantum/Migration/Reimport All AssetObjects", priority = 5007)]
  public static void ReimportAllAssetObjects() {
    // reimport all AssetObjects
    foreach (var assetPath in AssetDatabase.FindAssets($"t:{nameof(AssetObject)}")
              .Select(x => AssetDatabase.GUIDToAssetPath(x))) {
      AssetDatabase.ImportAsset(assetPath);
    }
  }
  
  [MenuItem("Tools/Quantum/Migration/Enable Code Upgrade", priority = 5100)]
  public static void EnableCodeUpgrade() {
    Log("Enabling code upgrade");
    if (SetScriptingDefine(EditorUserBuildSettings.selectedBuildTargetGroup, CodeUpgradeDefine, true)) {
      Log("Enabled code upgrade");
    } else {
      Log("Code upgrade already enabled");
    }
  }

  [MenuItem("Tools/Quantum/Migration/Run Upgrade CodeGen", priority = 5101)]
  public static void RunUpgradeCodeGen() {
    Log("Running upgrade code gen");
    QuantumCodeGenQtn.Run(verbose: true, options: new GeneratorOptions() { LegacyCodeGenOptions = GeneratorLegacyOptions.AssetRefs | GeneratorLegacyOptions.AssetBaseStubs | GeneratorLegacyOptions.AssetObjectAccessors | GeneratorLegacyOptions.UnderscorePrototypesSuffix | GeneratorLegacyOptions.BuiltInComponentPrototypeWrappers | GeneratorLegacyOptions.UnityUpgradableObsoleteAttributes });
  }

  [MenuItem("Tools/Quantum/Migration/Disable Code Upgrade", priority = 5102)]
  public static void DisableCodeUpgrade() {

#if QUANTUM_ENABLE_MIGRATION_CODE_UPGRADE
    QuantumUnityUpgradableOverrides.SetSourceUpdateFilters(false, out _);
#endif

    Log("Disabling code upgrade");
    if (SetScriptingDefine(EditorUserBuildSettings.selectedBuildTargetGroup, CodeUpgradeDefine, false)) {
      Log("Disabled code upgrade");
    } else {
      Log("Code upgrade already disabled");
    }
  }

  public static void ImportSimulationProject(string folder) {
    Log($"Importing from {folder}");

    var fullFolderPath = Path.GetFullPath(folder);
    const string ProjectFileName = "quantum.code.csproj";
    const string DestinationFolder = "Assets/QuantumUser/Simulation";


    if (!File.Exists(Path.Combine(fullFolderPath, ProjectFileName))) {
      throw new InvalidOperationException($"{ProjectFileName} not found in {folder}");
    }

    var ignoredFolders = new List<string> {
      Path.Combine(fullFolderPath, "bin"),
      Path.Combine(fullFolderPath, "obj"),
      Path.Combine(fullFolderPath, "BotSDK"),
    };

    var ignoredFiles = new List<string> {
      "Core.cs",
      "CodeGen.cs",
      "AssemblyInfo.cs"
    };

    var extensions = new[] { ".qtn", ".cs" };

    string[] filesToCopy = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
     .Where(file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".qtn", StringComparison.OrdinalIgnoreCase))
     .Where(file => !ignoredFolders.Any(dir => Path.GetDirectoryName(file).StartsWith(dir)) && !ignoredFiles.Contains(Path.GetFileName(file)))
     .ToArray();

    foreach (var file in filesToCopy) {
      var relativePath = Path.GetRelativePath(fullFolderPath, file);
      string destPath = Path.Combine(DestinationFolder, relativePath);

      string destDir = Path.GetDirectoryName(destPath);
      if (!Directory.Exists(destDir)) {
        Directory.CreateDirectory(destDir);
      }

      Log($"Copying from {file} to {destPath}");
      File.Copy(file, destPath, true);
    }

    Log($"Copied {filesToCopy.Length} files");
    
    // refresh DB for metas to generate
    AssetDatabase.Refresh();
  }

  private static int CheckAssetObjectScriptsBeingReadyInternal() {
    // check if the default assembly ("Assembly-CSharp") has any compile errors
    var defaultAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Assembly-CSharp");
    if (defaultAssembly == null) {
      Log("Default assembly is not loaded - there are likely compile errors.");
      return 1;
    }

    var assetObjectTypes = TypeCache.GetTypesDerivedFrom<AssetObject>()
     .Where(x => !x.IsGenericTypeDefinition && !x.IsAbstract)
     .ToList();

    bool success = true;

    var getScript = _getScript.Value;

    foreach (var type in assetObjectTypes) {
      if (type.GetCustomAttribute<IgnoreInAssetObjectCheckAttribute>() != null) {
        continue;
      }

      var error = CheckTypeScript(type, out _, requireMainAsset: false);
      if (!string.IsNullOrEmpty(error)) {
        Log(error);
        success = false;
      }
    }

    return success ? 0 : 2;
  }

  private static string GetCommandLineOption(string option) {
    var cmdArgs = System.Environment.GetCommandLineArgs();
    var optionIndex = Array.IndexOf(cmdArgs, option);
    if (optionIndex < 0 && optionIndex >= cmdArgs.Length - 1) {
      throw new InvalidOperationException($"Missing {option} argument");
    }

    return cmdArgs[optionIndex + 1];
  }

  static void ConsumeResult(bool success, [CallerMemberName] string memberName = null) {
    ConsumeResult(success ? 0 : 1, memberName);
  }
  
  static void ConsumeResult(int exitCode, [CallerMemberName] string memberName = null) {
    if (exitCode == 0) {
      Log($"{memberName} succeeded");
    } else {
      string msg = $"{memberName} failed, check previous log messages (result: {exitCode})";
      if (Application.isBatchMode) {
        Error(msg);
        EditorApplication.Exit(exitCode);
      } else {
        throw new InvalidOperationException(msg);
      }
    }
  }

  static void Log(string message) {
    Debug.Log($"[Quantum Migration] {message}");
  }

  static void Error(string message) {
    Debug.LogError($"[Quantum Migration] {message}");
  }

  static bool SetScriptingDefine(BuildTargetGroup group, string define, bool enable) {
    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(";");
    var index = Array.IndexOf(defines, define);

    if (index < 0) {
      if (enable) {
        ArrayUtility.Add(ref defines, define);
      } else {
        return false;
      }
    } else {
      if (enable) {
        return false;
      } else {
        ArrayUtility.RemoveAt(ref defines, index);
      }
    }

    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
    return true;
  }

  static IEnumerable<BuildTargetGroup> ValidBuildTargetGroups {
    get {
      foreach (var name in System.Enum.GetNames(typeof(BuildTargetGroup))) {
        // is the enum value obsolete?
        var fi = typeof(BuildTargetGroup).GetField(name);
        var attributes = fi.GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
        if (attributes?.Length > 0) {
          continue;
        }

        var group = (BuildTargetGroup)System.Enum.Parse(typeof(BuildTargetGroup), name);
        if (group == BuildTargetGroup.Unknown)
          continue;

        yield return group;
      }
    }
  }

  private static Lazy<Func<Type, MonoScript>> _getScript = new Lazy<Func<Type, MonoScript>>(() => {
    var methodName = "GetScript";
    var type = typeof(UnityEditor.EditorGUIUtility);
    var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic) ?? throw new ArgumentOutOfRangeException($"Method {type.FullName}.{methodName} not found");
    var getScriptDelegate = (Func<string, UnityEngine.Object>)methodInfo.CreateDelegate(typeof(Func<string, UnityEngine.Object>));
    return x => (MonoScript)getScriptDelegate(x.Name);
  });

  private static string CheckTypeScript(Type type, out MonoScript monoScript, bool requireMainAsset = true) {
    monoScript = _getScript.Value(type);
    if (monoScript == null) {
      return $"Failed to load MonoScript for {type.FullName}; is this type in a file matching its name ({type.Name}.cs)?";
    }

    if (monoScript.GetClass() != type) {
      return $"Invalid MonoScript for {type.FullName}: expected {type.FullName}, got {monoScript.GetClass()?.FullName}";
    }

    if (requireMainAsset) {
      if (!AssetDatabase.IsMainAsset(monoScript)) {
        return $"MonoScript for {type.FullName} is not a main asset";
      }
    }

    return string.Empty;
  }

  static UnityEngine.Object SetScriptableObjectType(UnityEngine.Object obj, Type type) {
    const string ScriptPropertyName = "m_Script";

    if (!obj) {
      throw new ArgumentNullException(nameof(obj));
    }

    if (type == null) {
      throw new ArgumentNullException(nameof(type));
    }

    if (!type.IsSubclassOf(typeof(ScriptableObject))) {
      throw new ArgumentException($"Type {type} is not a subclass of {nameof(ScriptableObject)}");
    }

    if (obj.GetType() == type) {
      return obj;
    }

    var tmp = ScriptableObject.CreateInstance(type);
    try {
      using (var dst = new SerializedObject(obj)) {
        using (var src = new SerializedObject(tmp)) {
          var scriptDst = dst.FindProperty(ScriptPropertyName);
          var scriptSrc = src.FindProperty(ScriptPropertyName);
          Debug.Assert(scriptDst != null);
          Debug.Assert(scriptSrc != null);
          Debug.Assert(scriptDst.objectReferenceValue != scriptSrc.objectReferenceValue);
          dst.CopyFromSerializedProperty(scriptSrc);
          dst.ApplyModifiedPropertiesWithoutUndo();
          return (ScriptableObject)dst.targetObject;
        }
      }
    } finally {
      UnityEngine.Object.DestroyImmediate(tmp);
    }
  }
}