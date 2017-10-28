using UnityEngine;

namespace TabletopCardCompanion.Debug
{
    public class ExitWithoutError : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}