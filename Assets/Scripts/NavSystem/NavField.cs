using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavSystem.Field
{
    public enum Zone
    {
        None,
        Up,
        Middle,
        Down
    }

    public class NavField : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Vector2 _leftUpCorner;
        [SerializeField] private Vector2 _rightDownCorner;
        [SerializeField] private Vector2 _offset;

        [SerializeField] private GameObject _upZone;
        [SerializeField] private GameObject _midZone;
        [SerializeField] private GameObject _downZone;

        [SerializeField] private LayerMask _cellLayer;

        [SerializeField] private static Vector3[] _upZonePoints;
        [SerializeField] private static Vector3[] _midZonePoints;
        [SerializeField] private static Vector3[] _downZonePoints;

        //TEMP
        private List<GameObject> _cells = new();

        private void Awake()
        {
            SetUpZones();
        }

        [ContextMenu("Generate field")]
        public void GenerateField()
        {
            _cells.ForEach(cell => { Destroy(cell); });

            List<GameObject> cells = new List<GameObject>();

            if (_offset.x <= 0 || _offset.y <= 0)
            {
                Debug.LogError("Offset must be greater than zero");
                return;
            }

            Vector2 cellScale = (Vector2)_cellPrefab.transform.lossyScale;

            Vector2 offsetScale = cellScale * _offset;

            Vector2 rightUpCorner = new Vector2(_rightDownCorner.x, _leftUpCorner.y);
            Vector2 leftDownCorner = new Vector2(_leftUpCorner.x, _rightDownCorner.y);

            int cellCountInRow = Mathf.CeilToInt((_rightDownCorner.x - _leftUpCorner.x - cellScale.x) / _offset.x);
            int cellCountInColumn = Mathf.CeilToInt((_leftUpCorner.y - _rightDownCorner.y - cellScale.y) / _offset.y);

            cellCountInRow = Mathf.Abs(cellCountInRow);
            cellCountInColumn = Mathf.Abs(cellCountInColumn);

            cells.AddRange(DrawField(StartPoint.LeftUp, _leftUpCorner, cellCountInRow, cellCountInColumn, cellScale, _offset));
            cells.AddRange(DrawField(StartPoint.RightUp, rightUpCorner, cellCountInRow, cellCountInColumn, cellScale, _offset));
            cells.AddRange(DrawField(StartPoint.LeftDown, leftDownCorner, cellCountInRow, cellCountInColumn, cellScale, _offset));
            cells.AddRange(DrawField(StartPoint.RightDown, _rightDownCorner, cellCountInRow, cellCountInColumn, cellScale, _offset));

            int removed = 0;

            List<GameObject> blackList = new();

            foreach (var cell1 in cells)
            {
                if (blackList.Contains(cell1))
                    continue;

                foreach (var cell2 in cells)
                {
                    if (cell1 != cell2 && cell1.transform.position == cell2.transform.position)
                    {
                        if (!blackList.Contains(cell2))
                            blackList.Add(cell2);
                    }
                }
            }

            blackList.ForEach((cell) => { Destroy(cell); removed++; });

            Debug.Log($"Removed: {removed} equal cells");

            //TEMP
            _cells = cells;
        }

        enum StartPoint
        {
            LeftUp,
            RightUp,
            LeftDown,
            RightDown,
        }

        private List<GameObject> DrawField(StartPoint startPoint, Vector2 startPointValue, int rowCount, int columnCount, Vector2 cellSize, Vector2 offsetSize)
        {
            List<GameObject> spawnedObj = new();

            int param1 = 1;
            int param2 = 1;
            int param3 = 1;

            switch (startPoint)
            {
                case StartPoint.LeftUp:
                    param1 = 1;
                    param2 = -1;
                    param3 = -1;
                    break;
                case StartPoint.RightUp:
                    param1 = -1;
                    param2 = -1;
                    param3 = 1;
                    break;
                case StartPoint.LeftDown:
                    param1 = 1;
                    param2 = 1;
                    param3 = 1;
                    break;
                case StartPoint.RightDown:
                    param1 = -1;
                    param2 = 1;
                    param3 = -1;
                    break;
            }

            for (int i = 0; i < rowCount; i++)
            {
                var rowCell = Instantiate(_cellPrefab, transform);
                rowCell.transform.position = startPointValue + new Vector2(offsetSize.x * i + cellSize.x / 2, cellSize.y / 2 * param3) * param1;
                spawnedObj.Add(rowCell);
                for (int j = 1; j < columnCount; j++)
                {
                    var columnCell = Instantiate(_cellPrefab, transform);
                    columnCell.transform.position = (Vector2)rowCell.transform.position + new Vector2(0, offsetSize.y * j) * param2;
                    spawnedObj.Add(columnCell);
                }
            }
            return spawnedObj;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere((Vector3)_leftUpCorner + Vector3.forward * transform.position.z, 0.5f);
            Gizmos.DrawSphere((Vector3)_rightDownCorner + Vector3.forward * transform.position.z, 0.5f);
        }

        public static Vector3[] GetZonePoints(Zone zone)
        {
            switch (zone)
            {
                case Zone.Up:
                    return _upZonePoints;
                case Zone.Middle:
                    return _midZonePoints;
                case Zone.Down:
                    return _downZonePoints;
                default:
                    return default;
            }
        }

        [ContextMenu("Bake zones")]
        private void SetUpZones()
        {
            SetZone(ref _upZonePoints, Zone.Up);
            SetZone(ref _midZonePoints, Zone.Middle);
            SetZone(ref _downZonePoints, Zone.Down);
        }

        private void SetZone(ref Vector3[] zonePoints, Zone zone)
        {
            RaycastHit2D[] hits;
            Transform zoneTransform = null;

            switch (zone)
            {
                case Zone.Up:
                    zoneTransform = _upZone.transform;
                    break;
                case Zone.Middle:
                    zoneTransform = _midZone.transform;
                    break;
                case Zone.Down:
                    zoneTransform = _downZone.transform;
                    break;
                default:
                    return;
            }

            hits = Physics2D.BoxCastAll(zoneTransform.position, zoneTransform.localScale, zoneTransform.rotation.z, Vector2.up, 0, _cellLayer);

            Vector3[] result = new Vector3[hits.Length];

            var i = 0;

            foreach (var hit in hits)
            {
                result[i] = hit.transform.position;
                i++;
            }

            zonePoints = result;
        }
    }
}