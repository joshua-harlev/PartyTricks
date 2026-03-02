using UnityEngine;

[CreateAssetMenu(fileName = "ShopStockConfig", menuName = "Scriptable Objects/ShopStockConfig")]
public class ShopStockConfig : ScriptableObject {
    public ShopStock[] StocksOrderedByRound;
}
