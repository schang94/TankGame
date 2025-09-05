using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class TurretCtrl : MonoBehaviourPun, IPunObservable
{
    Ray ray;
    RaycastHit hit;

    Transform tr;
    private float rotSpeed = 5f;
    float maxDistance = 100f;

    Quaternion curRot = Quaternion.identity;

    
    void Start()
    {
        tr = transform;
        curRot = tr.localRotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.localRotation);
        }
        else
        {
            curRot = (Quaternion)stream.ReceiveNext();
        }

    }
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 60f, 1 << 6))
            {
                Vector3 relative = tr.InverseTransformPoint(hit.point); // ������ ���� ������ ���� ��ǥ�� ���� ��ǥ�� ��ȯ
                                                                        // ����� = ��ź��Ʈ (��������.x, ��������.z) * PI * 2 / 360
                float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
                tr.Rotate(0f, angle * Time.deltaTime * rotSpeed, 0f);

            }
        }
        else
        {
            tr.localRotation = Quaternion.Lerp(tr.localRotation, curRot, Time.fixedDeltaTime * rotSpeed);
        }
    }
}
