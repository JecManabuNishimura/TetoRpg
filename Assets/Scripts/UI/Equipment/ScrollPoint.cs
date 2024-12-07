using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPoint : MonoBehaviour
{
   [SerializeField] private ScrollRect scrollRect;

   private void Start()
   {
       scrollRect.verticalScrollbar.value = 1;
       MovePoint();
   }

   private void Update()
   {
       MovePoint();
   }

   public void MovePoint()
   {
       Vector3 pos = transform.localPosition;
       float height = scrollRect.verticalScrollbar.GetComponent<RectTransform>().rect.height;
       float val = scrollRect.verticalScrollbar.gameObject.activeSelf ? scrollRect.verticalScrollbar.value : 1;
       pos.y = (height * val) - height / 2 ;
       transform.localPosition = pos;
   }
}
