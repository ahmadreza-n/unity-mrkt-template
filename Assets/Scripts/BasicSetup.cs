using UnityEngine;

public class BasicSetup : MonoBehaviour
{
  [SerializeField]
  [Tooltip("First Scene")]
  private string firstScene;

  // Start is called before the first frame update
  void Start()
  {
    Debug.LogFormat("Application persistent data path: {0}", Application.persistentDataPath);
    Debug.LogFormat("Application version: {0}", Application.version);
  }

  // Update is called once per frame
  void Update()
  {
  }
}
