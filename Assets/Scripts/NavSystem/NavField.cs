using System.Collections.Generic;
using UnityEngine;

namespace NavSystem.Field
{
    public class NavField : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Vector2 _leftUpCorner;
        [SerializeField] private Vector2 _rightDownCorner;
        [SerializeField] private Vector2 _offset;

        [ContextMenu("Generate field")]
        public void GenerateField()
        {
            List<GameObject> cells = new List<GameObject>();

            Vector2 cellScale = (Vector2)_cellPrefab.transform.lossyScale;

            if (_offset.y <= -cellScale.y)
                _offset.y = -cellScale.y + cellScale.x / 3;

            if (_offset.x <= -cellScale.x)
                _offset.x = -cellScale.x + cellScale.x / 3;

            Vector2 offsetScale = cellScale + _offset;

            Vector2 rightUpCorner = new Vector2(_rightDownCorner.x, _leftUpCorner.y);
            Vector2 leftDownCorner = new Vector2(_leftUpCorner.x, _rightDownCorner.y);


            int cellCountInRow = Mathf.FloorToInt((_rightDownCorner.x - _leftUpCorner.x) / offsetScale.x);
            int cellCountInColumn = Mathf.FloorToInt((_leftUpCorner.y - _rightDownCorner.y) / offsetScale.y);

            if(_offset.x < 0)
                cellCountInRow--;
            if (_offset.y < 0)
                cellCountInColumn--;

            Debug.Log(_rightDownCorner.x - _leftUpCorner.x);
            Debug.Log(offsetScale.x);
            Debug.Log(cellCountInRow);

            cells.AddRange(DrawField(StartPoint.LeftUp, _leftUpCorner, cellCountInRow, cellCountInColumn, cellScale, offsetScale));
            cells.AddRange(DrawField(StartPoint.RightUp, rightUpCorner, cellCountInRow, cellCountInColumn, cellScale, offsetScale));
            cells.AddRange(DrawField(StartPoint.LeftDown, leftDownCorner, cellCountInRow, cellCountInColumn, cellScale, offsetScale));
            cells.AddRange(DrawField(StartPoint.RightDown, _rightDownCorner, cellCountInRow, cellCountInColumn, cellScale, offsetScale));

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
                        if(!blackList.Contains(cell2))
                            blackList.Add(cell2);
                    }
                }
            }

            blackList.ForEach((cell) => { Destroy(cell); removed++;});

            Debug.Log($"Removed: {removed} equal cells");
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
            Gizmos.DrawSphere(_leftUpCorner, 0.4f);
            Gizmos.DrawSphere(_rightDownCorner, 0.4f);
        }
    }
}