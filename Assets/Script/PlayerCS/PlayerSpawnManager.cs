using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    private void Start()
    {
        // �V�[���ڍs���ɃJ�X�^���ʒu���ݒ肳��Ă���ꍇ�A�v���C���[�����̈ʒu�Ɉړ�
        if (DoorWarp.hasSpawnPosition)
        {
            transform.position = DoorWarp.nextSpawnPosition;

            // ��x�g�p�����烊�Z�b�g
            DoorWarp.hasSpawnPosition = false;

            Debug.Log($"�v���C���[���ʒu {DoorWarp.nextSpawnPosition} �Ɉړ����܂���");
        }
    }
}