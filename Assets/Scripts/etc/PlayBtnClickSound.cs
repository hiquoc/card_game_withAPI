using UnityEngine;

public class PlayBtnClickSound:MonoBehaviour
{
    public void PlaySound()
    {
        SoundManager.Instance.Play("btnClick");
    }
}