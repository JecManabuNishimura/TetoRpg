using DG.Tweening;
using UnityEngine;

public class CursorController : MonoBehaviour
{
   public void PlayAnim()
   {
      DOTween.Restart(gameObject);
      
   }
}

