using UnityEngine;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine.Networking;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public GameObject rickCharacter;
    public GameObject mortyCharacter;

    public Transform[] rickWaypoints;
    public Transform[] mortyWaypoints;

    private DialogueLine[] dialogue;
    private int currentLine = 0;
    private bool isPlaying = false;
    
    private Transform lastMortyWaypoint = null;
    private Transform lastRickWaypoint = null;
    void Start()
    {
        // Load dialogue
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "dialogue.json");
        string json = File.ReadAllText(jsonPath);
        dialogue = JsonHelper.FromJson<DialogueLine>(json);

        // Load and filter waypoints
        rickWaypoints = GameObject.Find("Rick waypoint")
            .GetComponentsInChildren<Transform>()
            .Where(t => t != null && t.name != "Rick waypoint")
            .ToArray();

        mortyWaypoints = GameObject.Find("Morty Waypoint")
            .GetComponentsInChildren<Transform>()
            .Where(t => t != null && t.name != "Morty Waypoint")
            .ToArray();

        // 🔁 Face each other at start
        FaceEachOther(rickCharacter, mortyCharacter);
    }
    private void FaceEachOther(GameObject a, GameObject b)
    {
        Vector3 dirToB = (b.transform.position - a.transform.position).normalized;
        dirToB.y = 0f;
        if (dirToB != Vector3.zero)
            a.transform.rotation = Quaternion.LookRotation(dirToB);

        Vector3 dirToA = (a.transform.position - b.transform.position).normalized;
        dirToA.y = 0f;
        if (dirToA != Vector3.zero)
            b.transform.rotation = Quaternion.LookRotation(dirToA);
    }


    public void BeginDialogue()
    {
        if (!isPlaying && dialogue != null && dialogue.Length > 0)
            StartCoroutine(PlayDialogue());
    }

    IEnumerator PlayDialogue()
    {
        isPlaying = true;

        while (currentLine < dialogue.Length)
        {
            DialogueLine line = dialogue[currentLine];
            string audioFileName = $"{line.speaker}_{currentLine:D4}";
            string audioPath = Path.Combine(Application.dataPath, "Resources/AudioClips", audioFileName + ".wav");
            string uri = "file://" + audioPath;

            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"❌ Failed to load: {audioFileName} — {www.error}");
                currentLine++;
                continue;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip == null)
            {
                Debug.LogWarning($"❌ Null clip for: {audioFileName}");
                currentLine++;
                continue;
            }

            // Determine speaker & listener
            bool isRickSpeaking = line.speaker.ToLower().Contains("rick");

            GameObject speaker = isRickSpeaking ? rickCharacter : mortyCharacter;
            GameObject listener = isRickSpeaking ? mortyCharacter : rickCharacter;

            AudioSource speakerAudio = speaker.GetComponent<AudioSource>();
            Animator speakerAnim = speaker.GetComponent<Animator>();
            Animator listenerAnim = listener.GetComponent<Animator>();

            // 🔊 Speaker starts talking
            if (speakerAnim != null)
                speakerAnim.SetBool("isTalking", true);

            speakerAudio.clip = clip;
            speakerAudio.Play();

            // Show subtitle
            subtitleText.text = $"{line.speaker}: {line.text}";

            // 🎭 Listener behavior (40% idle, 40% angry, 20% walk)
// 🎭 Listener behavior (10% idle, 40% angry, 50% walk)
            if (listenerAnim != null)
            {
                listenerAnim.SetBool("isTalking", false);
                listenerAnim.SetBool("isWalking", false);
                listenerAnim.SetBool("isAngry", false);

                int moodRoll = Random.Range(0, 100);

                if (moodRoll < 50) // 0–49 → 50% walk
                {
                    listenerAnim.SetBool("isWalking", true);

                    Transform[] waypoints = isRickSpeaking ? mortyWaypoints : rickWaypoints;
                    Transform lastUsed = isRickSpeaking ? lastMortyWaypoint : lastRickWaypoint;

                    Transform[] filteredWaypoints = waypoints.Where(wp => wp != lastUsed).ToArray();

                    if (filteredWaypoints.Length == 0)
                    {
                        // All waypoints filtered out — fallback to full list
                        filteredWaypoints = waypoints;
                    }

                    Transform chosenTarget = filteredWaypoints[Random.Range(0, filteredWaypoints.Length)];
                    StartCoroutine(MoveToTarget(listener, chosenTarget, speaker));

                    // Save the last used waypoint
                    if (isRickSpeaking)
                        lastMortyWaypoint = chosenTarget;
                    else
                        lastRickWaypoint = chosenTarget;

                }
                else if (moodRoll < 90) // 50–89 → 40% angry
                {
                    listenerAnim.SetBool("isAngry", true);
                }
                // else → 90–99 → 10% idle (do nothing)
            }

            // Wait for clip duration
            yield return new WaitForSeconds(clip.length);

            // 🔇 Stop talking
            if (speakerAnim != null)
                speakerAnim.SetBool("isTalking", false);

            // 🧘 Reset listener mood
            if (listenerAnim != null)
            {
                listenerAnim.SetBool("isWalking", false);
                listenerAnim.SetBool("isAngry", false);
            }

            // 👁 Speaker faces listener
            Vector3 dir = listener.transform.position - speaker.transform.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
                speaker.transform.rotation = Quaternion.LookRotation(dir);

            yield return new WaitForSeconds(0.3f);
            currentLine++;
        }

        subtitleText.text = "";
        isPlaying = false;
    }

    IEnumerator MoveToTarget(GameObject character, Transform target, GameObject otherCharacter, float speed = 2f)
    {
        Animator animator = character.GetComponent<Animator>();
        Transform transform = character.transform;

        if (animator != null)
            animator.SetBool("isWalking", true);

        // Face the destination
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        dirToTarget.y = 0f;
        if (dirToTarget != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dirToTarget);

        // Move towards the target
        Vector3 startYLockedTarget = new Vector3(target.position.x, transform.position.y, target.position.z);

        while (Vector3.Distance(transform.position, startYLockedTarget) > 0.1f)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, startYLockedTarget, speed * Time.deltaTime);
            // Lock Y to current position
            nextPos.y = transform.position.y;
            transform.position = nextPos;
            yield return null;
        }

        // Stop walking
        if (animator != null)
            animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(0.1f);

        // Face the other character (the speaker)
        Vector3 dirToOther = (otherCharacter.transform.position - transform.position).normalized;
        dirToOther.y = 0f;
        if (dirToOther != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dirToOther);
    }
}
