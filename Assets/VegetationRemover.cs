using UnityEngine;

public class VegetationRemover : MonoBehaviour
{
    void Start()
    {
        int layer = LayerMask.NameToLayer("Trees");
        int layerMask = 1 << layer;
        Collider[] vegetationColliders = Physics.OverlapSphere(transform.position, 7f, layerMask);

        foreach (Collider collider in vegetationColliders)
        {
            Destroy(collider.gameObject);
        }
    }
}
