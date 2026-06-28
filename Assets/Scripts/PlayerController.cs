using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    // public GameObject boosterFlame;

    public UIDocument uiDocument;
    private Label scoreText;
    private Label highScoreText;
    private Button restartButton;

    private int highScore;
    private bool hasBeatenHighScore = false;
    private bool isFirstPlaythrough = false;

    public GameObject explosionEffect;

    public GameObject bubbleEffect;
    private ParticleSystem bubbleParticles;
    private AudioSource bubbleAudio;

    public GameObject borderParent;

    Rigidbody2D rb;

    private float elapsedTime = 0f;
    private float score = 0f;

    public float scoreMultiplier = 10f;
    public float thrustForce = 1f;
    public float maxSpeed = 5f;

    public int flashCount = 3;
    public float flashInterval = 0.2f;

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreLabel");

        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");
        restartButton.clicked += ReloadScene;

        restartButton.style.display = DisplayStyle.None;

        isFirstPlaythrough = !PlayerPrefs.HasKey("highScore");
        highScore = PlayerPrefs.GetInt("highScore", 0);

        highScoreText = uiDocument.rootVisualElement.Q<Label>("HighScoreLabel");
        highScoreText.text = $"High Score: {highScore}";

        bubbleParticles = bubbleEffect.GetComponent<ParticleSystem>();
        bubbleAudio = bubbleEffect.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        UpdateScore();
        MovePlayer();
    }

    void UpdateScore()
    {
        elapsedTime += Time.deltaTime;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier);
        scoreText.text = $"Score: {score}";

        if (score > highScore)
        {
            highScore = (int)score;
            highScoreText.text = $"High Score: {highScore}";

            // Only flash if this isn't the player's very first run
            if (!hasBeatenHighScore && !isFirstPlaythrough)
            {
                uiDocument.StartCoroutine(FlashHighScore());
            }

            hasBeatenHighScore = true;
        }
    }

    void MovePlayer()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        Vector2 direction = (mousePos - transform.position).normalized;

        transform.up = direction;

        if (Mouse.current.leftButton.isPressed)
        {
            rb.AddForce(direction * thrustForce);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            bubbleParticles.Play();
            bubbleAudio.pitch = Random.Range(0.95f, 1.05f);
            bubbleAudio.Play();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            bubbleParticles.Stop();
            bubbleAudio.Stop();
        }
    }

    IEnumerator FlashHighScore()
    {
        for (int i = 0; i < flashCount; i++)
        {
            highScoreText.style.opacity = 0;
            yield return new WaitForSeconds(flashInterval);

            highScoreText.style.opacity = 1;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        isDead = true;

        Instantiate(explosionEffect, transform.position, transform.rotation);
        restartButton.style.display = DisplayStyle.Flex;

        if (hasBeatenHighScore)
        {
            PlayerPrefs.SetInt("highScore", highScore);
            PlayerPrefs.Save();
        }

        borderParent.SetActive(false);
        Destroy(gameObject);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}