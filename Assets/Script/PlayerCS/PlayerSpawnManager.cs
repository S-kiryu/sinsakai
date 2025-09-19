using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    private void Start()
    {
        // シーン移行時にカスタム位置が設定されている場合、プレイヤーをその位置に移動
        if (DoorWarp.hasSpawnPosition)
        {
            transform.position = DoorWarp.nextSpawnPosition;

            // 一度使用したらリセット
            DoorWarp.hasSpawnPosition = false;

            Debug.Log($"プレイヤーを位置 {DoorWarp.nextSpawnPosition} に移動しました");
        }
    }
}