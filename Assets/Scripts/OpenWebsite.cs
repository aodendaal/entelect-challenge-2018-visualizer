using UnityEngine;

public class OpenWebsite : MonoBehaviour
{
    public string url;

    public void OpenWebsite_Click()
    {
        Application.OpenURL(url);
    }
}