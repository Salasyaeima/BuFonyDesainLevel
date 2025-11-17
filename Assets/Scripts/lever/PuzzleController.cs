using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    public Lever lever1;
    public Lever lever2;
    public Lever lever3;
    public Lever lever4;

    public Animator winObjectAnimator;

    private void OnEnable()
    {
        Lever.OnAnyLeverChanged += CheckCombination;
    }

    private void OnDisable()
    {
        Lever.OnAnyLeverChanged -= CheckCombination;
    }

    void CheckCombination()
    {
        if (lever1.IsTriggered == true &&
            lever2.IsTriggered == true &&
            lever3.IsTriggered == false &&
            lever4.IsTriggered == false)
        {
            winObjectAnimator.SetTrigger("Win");
        }
    }
}
