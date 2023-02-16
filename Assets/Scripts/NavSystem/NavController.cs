using System.Threading.Tasks;
using UnityEngine;
using NavSystem.Field;
using Unity.Collections;

namespace NavSystem.Controller
{
    public class NavController : MonoBehaviour
    {
        [SerializeField] private float _detectDistance;
        [SerializeField] private float _desideTime;
        [SerializeField] private LayerMask _cellLayer;
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private Vector3 _targetPoint;

        private Vector2 _moveDirection = Vector2.up;
        private Zone _currentZone = Zone.None;

        private async void Start()
        {
            _currentZone = (Zone)Random.Range(1, 3);
            Vector3[] zonePoints = NavField.GetZonePoints(_currentZone);
            _targetPoint = zonePoints[Random.Range(0, zonePoints.Length)];

            while (true)
            {
                await MoveTo(CheckPoints(_targetPoint));
                await Task.Delay(Mathf.FloorToInt(_desideTime * 1000));
            }
        }

        //gets all points in the range of detect distance
        public Vector2 CheckPoints()
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _detectDistance, Vector2.zero, 0, _cellLayer);

            if (hits.Length == 0)
                return transform.position;

            int randPoint = Random.Range(0, hits.Length - 1);

            return hits[randPoint].transform.position;
        }
        public Vector2 CheckPoints(Vector3 target)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _detectDistance, Vector2.zero, 0, _cellLayer);

            Vector3 nearest = transform.position;
            float minMagnitude = ((Vector2)target - (Vector2)nearest).magnitude;

            if (hits.Length == 0)
                return nearest;

            if (minMagnitude < _detectDistance)
                return _targetPoint;

            foreach (var hit in hits)
            {
                if (((Vector2)target - (Vector2)hit.transform.position).magnitude < minMagnitude)
                {
                    nearest = hit.transform.position;
                    minMagnitude = ((Vector2)target - (Vector2)nearest).magnitude;
                }
            }

            return nearest;
        }

        //Move to attached point 
        private async Task MoveTo(Vector2 targetPoint)
        {
            

            while ((Vector2)transform.position != targetPoint)
            {
                _moveDirection = (targetPoint - (Vector2)transform.position).normalized;
                transform.Translate(_moveDirection * 2f * Time.deltaTime);

                //Do raycast while move to attached point
                RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDirection, 2.5f, _enemyLayer);

                //if (hit.collider != null)
                //change point

                await Task.Yield();

                if ((targetPoint - (Vector2)transform.position).magnitude < 0.01f)
                    transform.position = targetPoint;
            }

            if (transform.position == _targetPoint)
            {
                Zone newZone = (Zone)Random.Range(1, 3);

                if (newZone != _currentZone)
                    _currentZone = newZone;
                else
                    switch (newZone)
                    {
                        case Zone.Up:
                            _currentZone = Zone.Middle;
                            break;
                        case Zone.Middle:
                            _currentZone = Zone.Down;
                            break;
                        case Zone.Down:
                            _currentZone = Zone.Middle;
                            break;
                    }

                Vector3[] zonePoints = NavField.GetZonePoints(_currentZone);
                _targetPoint = zonePoints[Random.Range(0, zonePoints.Length)];
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _moveDirection * 2.5f);
        }
    }
}
