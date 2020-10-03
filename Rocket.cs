using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 500f;
    [SerializeField] float mainThrust = 30f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip Death;
    [SerializeField] AudioClip Success;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem DeathParticles;
    [SerializeField] ParticleSystem SuccessParticles;
    
    Rigidbody rigidBody;
    AudioSource audioSource;
    
    bool collisionsDisabled = false;
    bool isTransitioning = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }
        if(Debug.isDebugBuild)
            RespondToDebugKeys();
    }
    private void RespondToDebugKeys()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
        else if(Input.GetKeyDown(KeyCode.E))
        {
            Application.Quit();
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (isTransitioning || collisionsDisabled) { return; }
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }
    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
            nextSceneIndex = 0;
        SceneManager.LoadScene(nextSceneIndex);
    }
    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }
    private void StartSuccessSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(Success);
        SuccessParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }
    private void StartDeathSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(Death);
        DeathParticles.Play();
        Invoke("LoadPreviousLevel", levelLoadDelay);
    }
    private void LoadPreviousLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;
        if (previousSceneIndex < 0)
            previousSceneIndex = 0;
        SceneManager.LoadScene(previousSceneIndex);
    }
    private void RespondToRotateInput()
    {
        rigidBody.angularVelocity = Vector3.zero;
        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.forward * rotationThisFrame);
        else if (Input.GetKey(KeyCode.D))
            transform.Rotate(-Vector3.forward * rotationThisFrame);
    }
    private void RespondToThrustInput()
    {
        rigidBody.freezeRotation = true;

        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }

        rigidBody.freezeRotation = false;
    }
    private void StopApplyingThrust()
    {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }
    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust);
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(mainEngine);
        mainEngineParticles.Play();
    }
}
