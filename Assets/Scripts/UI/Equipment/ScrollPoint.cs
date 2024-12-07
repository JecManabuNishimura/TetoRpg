using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPoint : MonoBehaviour
{
   [SerializeField] private ScrollRect scrollRect;

   private void Start()
   {
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
       pos.y = (height * scrollRect.verticalScrollbar.value) - height / 2 ;
       transform.localPosition = pos;
   }
}
