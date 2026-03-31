using UnityEngine;

public abstract class tileAction : ScriptableObject
{
    public LocalizedText actionName;
    public string actionDebugMessage;
    
    public abstract void affectTile(gameTile tile);
}
