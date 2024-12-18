using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewTextAnim : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    
    void Start()
    {
        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(textMesh);
        var sequence = DOTween.Sequence();
        sequence.SetLoops(-1);

        var duration = 0.2f;
        for (int i = 0; i < animator.textInfo.characterCount; i++)
        {
            
            sequence.Join(DOTween.Sequence()
                .Append(animator.DOOffsetChar(i,animator.GetCharOffset(i) + new Vector3(0,1,0),duration).SetEase(Ease.OutFlash,2))
                .Join(animator.DOScaleChar(i, 1.2f, duration).SetEase(Ease.OutFlash,2))
                //.Join(animator.DOScaleChar(i, 1f, duration * 0.3f).SetEase(Ease.OutBack))
                .Join(animator.DOColorChar(i,Color.yellow,duration*0.5f).SetLoops(2,LoopType.Yoyo))
                .AppendInterval(0.1f)
                .SetDelay(0.1f *i)
            );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
