using UnityEngine;
using TMPro;

namespace CardsVR.Interaction
{
    public class UpdateScoreLabel : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private int PileNum;
        private TextMeshPro Score;

        private void Start()
        {
            Score = GetComponent<TextMeshPro>();
        }

        void Update()
        {
            int score = GameManager.Instance.getNumCards(PileNum);
            Score.text = score.ToString();
        }
    }
}
