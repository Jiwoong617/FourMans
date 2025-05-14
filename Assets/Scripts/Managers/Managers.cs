using UnityEngine;

public class Managers : Singleton<Managers>
{
    private UIManager _ui = new UIManager();
    private ResourceManager _resource = new ResourceManager();

    public static UIManager UI { get { return Instance._ui; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
}
    