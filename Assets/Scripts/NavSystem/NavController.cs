using System.Threading.Tasks;
using UnityEngine;

namespace NavSystem.Controller
{
    public class NavController : MonoBehaviour
    {
        [SerializeField] private float _detectDistance;
        [SerializeField] private float _desideTime;
        [SerializeField] private LayerMask cellLayer;

        private async void Start()
        {
            while(true)
            {
                await MoveTo(CheckPoints());
                await Task.Delay(Mathf.FloorToInt(_desideTime * 1000));
            }
        }

        [ContextMenu("DestroyOverlaped")]
        public Vector2 CheckPoints()
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _detectDistance, Vector2.zero, 0, cellLayer);

            Debug.Log(hits.Length);

            if (hits.Length == 0)
                return transform.position;

            int randPoint = Random.Range(0, hits.Length - 1);

            return hits[randPoint].transform.position;
        }
        private async Task MoveTo(Vector2 targetPoint)
        {
            while ((Vector2)transform.position != targetPoint)
            {
                transform.Translate((targetPoint - (Vector2)transform.position).normalized * 2f * Time.deltaTime);
                
                await Task.Yield();
                
                if ((targetPoint - (Vector2)transform.position).magnitude < 0.01f)
                    transform.position = targetPoint;
            }
        }
    }
}
