using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class EnemyPickerUI : MonoBehaviour
{
  [System.Serializable]
  public struct Picker
  {
    public GameObject UIItem;
    public AssetRef<EntityPrototype> Prefab;
  }

  public static EnemyPickerUI Instance;

  public List<Picker> Items;
  public GameObject SelectionBorder;

  private int _selectedItem;

  private void Awake()
  {
    if(Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(this.gameObject);
    }
  }

  public void OnItemSelected(int id)
  {
    _selectedItem = id;

    SelectionBorder.transform.SetParent(Items[id].UIItem.transform);
    SelectionBorder.transform.localPosition = Vector3.zero;
  }

  public AssetRef<EntityPrototype> GetSelectedPrefab()
  {
    return Items[_selectedItem].Prefab;
  }
}
