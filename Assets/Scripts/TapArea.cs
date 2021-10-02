using UnityEngine;
using UnityEngine.EventSystems;

public class TapArea : MonoBehaviour, IPointerDownHandler
{
    public AudioClip coin;

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.CollectByTap(eventData.position, transform);
        GetComponent<AudioSource>().PlayOneShot(coin);
    }
}
