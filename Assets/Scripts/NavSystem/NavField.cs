using System.Collections.Generic;
using UnityEngine;

namespace NavSystem.Field
{
    public class NavField : MonoBehaviour
    {
        [SerializeField] private RectTransform _gameArea;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private int _layersCount;

        [ContextMenu("Generate field")]
        public void SetUpField()
        {
            Vector2 fieldSize = _gameArea.rect.size / 2;
            Vector2 origin = _gameCamera.WorldToScreenPoint(_gameArea.position);

            //Right up angle
            Vector2 RightUpPos = _gameCamera.ScreenToWorldPoint(origin + fieldSize);
            //Left down angle
            Vector2 LeftDownPos = _gameCamera.ScreenToWorldPoint(origin - fieldSize);

            Vector2 cellSize = _gameCamera.ScreenToWorldPoint((Vector2)_cellPrefab.transform.localScale
                             + new Vector2(_gameCamera.pixelWidth, _gameCamera.pixelHeight) / 2);

            Vector2 offset = cellSize / _layersCount;

            List<GameObject> rows = new();
            List<GameObject> columns = new();

            for (int i = 0; i < _layersCount; i++)
            {
                int cellCountInRow = Mathf.FloorToInt((_gameArea.rect.width - _cellPrefab.transform.localScale.x/_layersCount * i) / _cellPrefab.transform.localScale.x);
                int cellCountInColumn = Mathf.FloorToInt((_gameArea.rect.height - _cellPrefab.transform.localScale.y / _layersCount * i) / _cellPrefab.transform.localScale.y);

                List<GameObject> currentRows = new();
                currentRows.AddRange(DrowRow(new Vector2(LeftDownPos.x + offset.x * i, RightUpPos.y), cellSize, cellCountInRow, IsReverse: false));
                currentRows.AddRange(DrowRow(RightUpPos - new Vector2(offset.x * i, 0), cellSize, cellCountInRow, IsReverse: true));
                rows.AddRange(currentRows);

                //Dorow columns 
                foreach (var cell in currentRows)
                {
                    columns.AddRange(DrowColumn(new Vector2(cell.transform.position.x, RightUpPos.y - (offset.y * i)), cellSize, cellCountInColumn, IsReverse: false));
                    columns.AddRange(DrowColumn(new Vector2(cell.transform.position.x, LeftDownPos.y + (offset.y * i)), cellSize, cellCountInColumn, IsReverse: true));
                }
            }
        }

        private List<GameObject> DrowRow(Vector2 startPosition, Vector2 cellSize, int cellCount, bool IsReverse)
        {
            List<GameObject> result = new();
            for (int i = 0; i < cellCount; i++)
            {
                GameObject cellGO = Instantiate(_cellPrefab, _gameArea);
                cellGO.transform.position = new Vector2(startPosition.x + ((i * cellSize.x) + (cellSize / 2).x) * (IsReverse ? -1 : 1), startPosition.y - (cellSize / 2).y);
                result.Add(cellGO);
            }
            return result;
        }
        private List<GameObject> DrowColumn(Vector2 startPosition, Vector2 cellSize, int cellCount, bool IsReverse)
        {
            List<GameObject> result = new();
            for (int i = 0; i < cellCount; i++)
            {
                GameObject cellGO = Instantiate(_cellPrefab, _gameArea);
                cellGO.transform.position = new Vector2(startPosition.x, startPosition.y + ((i * cellSize.y) + (cellSize / 2).y) * (IsReverse ? 1 : -1));
                result.Add(cellGO);
            }
            return result;
        }
    }
}