using UnityEngine;
using System.Linq;

public class Artifacts : MonoBehaviour
{
    public static Artifacts Inst;
    public static GameObject[] artifacts;
    public GameObject prefab;

    private void Awake()
    {
        if (Inst != null)
        {
            Debug.LogError("Instance может быть только один");
            return;
        }
        Inst = this;
    }

    public void MoveArtifact(Vector2 _artifactPosition, Vector2 _newPosition)
    {
        artifacts.First(x => x.GetComponent<ArtifactScript>().position == _artifactPosition).transform.position = _newPosition;
    }

    public void GetArtefacts(Packet _packet)
    {
        artifacts = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            Vector2 artifactPos = _packet.ReadVector();
            artifacts[i] = Instantiate(prefab, FIeld.Inst[artifactPos.y, artifactPos.x].transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity);
            artifacts[i].GetComponent<ArtifactScript>().position = artifactPos;
        }
    }
}
