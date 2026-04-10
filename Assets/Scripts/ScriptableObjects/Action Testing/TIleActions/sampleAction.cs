using UnityEngine;

[CreateAssetMenu(fileName = "TileAction", menuName = "TileAction")]
public class sampleAction : tileAction
{
    public override bool affectTile(gameTile tile)
    {
        Debug.Log(actionDebugMessage);
        return true;
    }
}
