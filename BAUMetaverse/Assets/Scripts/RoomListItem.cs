
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = _info.Name;     
    }

    public void SetVisiblePasswordMenu(bool isVisible)
    {
        RoomManager.Instance.SetVisiblePasswordMenu(isVisible);
        RoomManager.Instance.info = info;
    }
}
