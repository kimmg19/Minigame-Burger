using UnityEngine;

public class EvalueateBurger : MonoBehaviour
{
    [Header("Evaluation Settings")]
    [SerializeField] private float fastTimeThreshold = 30f;
    [SerializeField] private int speedBonus = 5;
    [SerializeField] private float perfectHealthGain = 0.5f;
    [SerializeField] private float poorHealthLoss = -1f;

    [Header("Legacy Data")]
    public Scoredata[] scoredata;
    public BurgerRecipe[] burgerRecipes;

    private GameObject submitedBurger;
    private OrderController orderController;
    private CustomerOrderSystem customerOrderSystem;
    private CustomerOrderInfo customerOrderInfo;

    private void Start()
    {
        orderController = FindObjectOfType<OrderController>();
        customerOrderSystem = FindObjectOfType<CustomerOrderSystem>();
        submitedBurger = GameObject.Find("DropArea");
    }

    public void OnEvalue()
    {
        customerOrderSystem.IsMaking = false;

        // 1. 버거 데이터 수집
        var droppable = submitedBurger?.GetComponent<IDroppable>();
        if (droppable == null) return;

        BurgerData submittedBurgerData = droppable.GetBurgerData();

        // 2. 타겟 레시피
        int requestBurgerNum = orderController.GetburgerId();
        if (requestBurgerNum >= burgerRecipes.Length) return;

        BurgerRecipe targetRecipe = burgerRecipes[requestBurgerNum];

        // 3. 평가 실행
        EvaluationResult result = EvaluateBurger(submittedBurgerData, targetRecipe);

        // 4. 결과 적용
        ApplyResult(result);

        // 5. 다음 주문
        orderController?.NewOrder();
    }

    private EvaluationResult EvaluateBurger(BurgerData burger, BurgerRecipe recipe)
    {
        // 정확도 계산
        float accuracy = recipe.CalculateAccuracy(burger);
        bool isPerfect = recipe.MatchesExactly(burger);

        // 점수 계산
        int baseScore = isPerfect ? recipe.perfectScore :
                       accuracy >= 0.7f ? recipe.goodScore : recipe.poorScore;

        // 시간 보너스
        float timeTaken = GameManager.instance.takenTime;
        int finalScore = (timeTaken <= fastTimeThreshold && baseScore > 0) ?
                        baseScore + speedBonus : baseScore;

        // 체력 변화
        float healthChange = isPerfect ? perfectHealthGain :
                           accuracy < 0.3f ? poorHealthLoss : 0f;

        // 피드백
        string feedback = GenerateFeedback(accuracy, isPerfect, timeTaken);

        return new EvaluationResult
        {
            isPerfect = isPerfect,
            accuracy = accuracy,
            score = finalScore,
            healthChange = healthChange,
            feedback = feedback
        };
    }

    private string GenerateFeedback(float accuracy, bool isPerfect, float timeTaken)
    {
        if (isPerfect)
        {
            return timeTaken <= fastTimeThreshold ?
                   "Perfect and fast! Outstanding!" :
                   "Perfect burger! Customer is delighted!";
        }
        else if (accuracy >= 0.7f)
        {
            return "Good job! Customer is satisfied.";
        }
        else
        {
            return "Customer is disappointed...";
        }
    }

    private void ApplyResult(EvaluationResult result)
    {
        // 점수 및 체력 적용
        GameManager.instance.score += result.score;
        GameManager.instance.health += result.healthChange;

        // 대화 설정
        GameManager.instance.Dialog = result.feedback;

        // 고객 표정
        customerOrderInfo = FindObjectOfType<CustomerOrderInfo>();
        if (customerOrderInfo != null)
        {
            if (result.isPerfect || result.accuracy >= 0.7f)
                customerOrderInfo.SetCustomerHappyFace();
            else if (result.accuracy >= 0.3f)
                customerOrderInfo.SetCustomerNormalFace();
            else
                customerOrderInfo.SetCustomerSadFace();
        }

        // 사운드
        if (SoundManager.instance != null)
        {
            if (result.score > 0)
                SoundManager.instance.PlaySFX(SoundManager.ESfx.SFX_SCORE);
            else
                SoundManager.instance.PlaySFX(SoundManager.ESfx.SFX_LOSTHEALTH);
        }

        // 시간 초기화
        GameManager.instance.takenTime = 0;
    }

    // 기존 메서드들 (호환성)
    public Scoredata GetScoredatas(int n)
    {
        return n >= 0 && n < scoredata.Length ? scoredata[n] : null;
    }

    public bool IsPerferctBurger(GameObject items, BurgerRecipe recipe)
    {
        var droppable = items.GetComponent<IDroppable>();
        if (droppable == null) return false;

        var burgerData = droppable.GetBurgerData();
        return recipe.MatchesExactly(burgerData);
    }
}