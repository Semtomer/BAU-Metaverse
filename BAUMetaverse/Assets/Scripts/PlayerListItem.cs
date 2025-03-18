
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;

    Player player;

    [SerializeField] Sprite Joe;
    [SerializeField] Sprite Jolleen;
    [SerializeField] Sprite Leonard;
    [SerializeField] Sprite Louise;
    [SerializeField] Sprite Sophie;
    [SerializeField] Sprite TheBoss;
    [SerializeField] Image img;

    public void SetUp(Player _player, string selectedCharacter)
    {
        player = _player;
        text.text = _player.NickName;

        switch (selectedCharacter)
        {
            case "Joe": img.sprite = Joe; break;
            case "Jolleen": img.sprite = Jolleen; break;
            case "Leonard": img.sprite = Leonard; break;
            case "Louise": img.sprite = Louise; break;
            case "Sophie": img.sprite = Sophie; break;
            case "TheBoss": img.sprite = TheBoss; break;
            default: break;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
