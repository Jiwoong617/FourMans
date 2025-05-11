using UnityEngine;

public class Managers : Singleton<Managers>
{
    private UIManager _ui;
    private ResourceManager _resource;

    public static UIManager UI { get { return Instance._ui; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
}
    