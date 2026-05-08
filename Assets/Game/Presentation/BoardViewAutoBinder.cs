using System;
using System.Reflection;
using UnityEngine;

namespace Game.Presentation
{
    [DefaultExecutionOrder(-1000)]
    public sealed class BoardViewAutoBinder : MonoBehaviour
    {
        [SerializeField] private GameObject fallbackNatureLightTilePrefab;
        [SerializeField] private string resourcesPrefabName = "NatureLightTile";

        private void Awake()
        {
            try
            {
                var boardView = GetComponent("BoardView");
                if (boardView == null) return;

                var type = boardView.GetType();
                var field = type.GetField("natureLightTilePrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                {
                    Debug.LogError("[FBL] BoardViewAutoBinder: Field 'natureLightTilePrefab' not found on BoardView.");
                    return;
                }

                var current = field.GetValue(boardView) as GameObject;
                if (current != null) return;

                GameObject prefab = fallbackNatureLightTilePrefab;
                if (prefab == null)
                {
                    prefab = Resources.Load<GameObject>(resourcesPrefabName);
                }

                if (prefab == null)
                {
                    throw new MissingReferenceException("[FBL] BoardViewAutoBinder failed: NatureLightTile prefab not found. Expected Resources/" + resourcesPrefabName + ".prefab or fallback reference.");
                }

                field.SetValue(boardView, prefab);
                Debug.Log("[FBL] BoardViewAutoBinder assigned NatureLightTile prefab automatically.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[FBL] BoardViewAutoBinder exception: " + ex);
                throw;
            }
        }
    }
}
