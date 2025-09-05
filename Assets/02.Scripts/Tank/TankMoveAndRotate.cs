using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;

using UnityEngine;

public class TankMoveAndRotate : MonoBehaviourPun, IPunObservable
{
    public float moveSpeed = 12f;
    public float rotSpeed = 90f;
    Transform tr;
    Rigidbody rb;
    TankInput input;

    Vector3 curPos = Vector3.zero;
    Quaternion curRot = Quaternion.identity;
    IEnumerator Start()
    {
        tr = transform;
        input = GetComponent<TankInput>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        curPos = tr.position;
        curRot = tr.rotation;
        yield return null;
        if (photonView != null)
        {
            if (photonView.IsMine) // �����Ʈ��ũ�� ����䰡 ���ǰ��̶��
            {
                CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
                vcam.Follow = transform;
                vcam.LookAt = transform;
            }
        }
        
    }

    // ������ �̵� ȸ���� ��Ʈ��ũ ���� Ÿ���� ����Ʈ�� �۽��ϰ�
    // �ݴ�� ����Ʈ�� �̵� ȸ���� ���� �޾Ƽ� ��Ʈ��ũ �� ������ ���� ������ �ϱ� ������
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // �ڽ��� ��ũ �������� �۽�
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
        else // ����Ʈ�� �������� ����
        {
            curPos = (Vector3) stream.ReceiveNext();
            curRot = (Quaternion) stream.ReceiveNext();
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (GameManager.Instance.isGameOver) return;
            tr.Translate(Vector3.forward * input.v * moveSpeed * Time.fixedDeltaTime);
            tr.Rotate(Vector3.up * input.h * rotSpeed * Time.fixedDeltaTime);
        }
        else
        {
            tr.localPosition = Vector3.Lerp(tr.localPosition, curPos, Time.fixedDeltaTime * moveSpeed);
            tr.localRotation = Quaternion.Lerp(tr.localRotation, curRot, Time.fixedDeltaTime * rotSpeed);
        }
        
    }
}
