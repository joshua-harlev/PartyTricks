using UnityEngine;

// List of items for a given shop iteration
[CreateAssetMenu(fileName = "ShopStock", menuName = "Scriptable Objects/Shop Stock Preset")]
[System.Serializable]
public class ShopStock : ScriptableObject {
    [Tooltip("Max 4 items. Order is left-to-right, top-to-bottom. If <4 items, fourth item will be No Item and other blank slots will be random.")]
    public ShopItem[] Items;
}
