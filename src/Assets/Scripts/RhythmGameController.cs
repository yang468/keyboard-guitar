using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
using UnityEngine.UI;
using SonicBloom.Koreo.Players;
using UnityEngine.SceneManagement;
public class RhythmGameController : MonoBehaviour
{
    //����Ŀ�����ɵĹ�����¼���ӦID
    [EventID]
    public string eventID;
    public float noteSpeed = 1;     //�����ٶ�
    [Tooltip("�����������䴰��")]
    [Range(8f, 300f)]
    public float hitWindowRangeInMS;    //���������������
    //��UnityΪ��λ���ʵ�ǰ���д��ڵĴ�С
    public float WindowSizeInUnity
    {
        get
        {
            return noteSpeed * (hitWindowRangeInMS * 0.001f);
        }
    }
    //�����������е����д���
    int hitWindowRangeInSamples;
    public int HitWindowSampleWidth
    {
        get
        {
            return hitWindowRangeInSamples;
        }
    }
    public int SampleRate
    {
        get
        {
            return playingKoreo.SampleRate;
        }
    }
    //����
    Koreography playingKoreo;
    public AudioSource audioCom;

    public List<LineController> noteLanes = new List<LineController>();

    SimpleMusicPlayer simpleMusicPlayer;
    public Transform simpleMusicPlayerTrans;
    public GameObject gameOverUIGo;
    //����
    [Tooltip("��ʼ������Ƶ֮ǰ�ṩ��ʱ��������ǰ����ʱ��")]
    public float leadInTime;
    //��Ƶ����֮ǰ��ʣ��ʱ����
    float leadInTimeLeft;
    //���ֿ�ʼ֮ǰ�ļ�ʱ��
    float timeLeftToPlay;
    Stack<NoteObject> noteObjectPool = new Stack<NoteObject>();
    public Stack<GameObject> downEffectObjectPool = new Stack<GameObject>();
    public Stack<GameObject> hitEffectObjectPool = new Stack<GameObject>();
    public Stack<GameObject> hitLongNoteEffectObjectPool = new Stack<GameObject>();
    //Ԥ������Դ
    //����
    public NoteObject noteObject;
    //������Ч
    public GameObject downEffectGo;
    //������Ч
    public GameObject hitEffectGo;
    //���г�������Ч
    //public GameObject hitLongNoteEffectGo;

    public int DelayedSampleTime
    {
        get
        {
            return playingKoreo.GetLatestSampleTime() - (int)(SampleRate *leadInTimeLeft);
        }
    }
    public int score;
    public bool isPauseState;
    bool gameStart;
    //UI
    public Text scoreText;
    public GameObject gameOverUI;
    //��Դ ��������
    public Koreography kgy;
    // Start is called before the first frame update
    void Start()
    {
        InitializeLeadIn();
        simpleMusicPlayer = simpleMusicPlayerTrans.GetComponent<SimpleMusicPlayer>();
        simpleMusicPlayer.LoadSong(kgy,0,false);
        for (int i = 0; i < noteLanes.Count; i++)
        {
            noteLanes[i].Initialize(this);
        }
        playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0);
        //��ȡ�¼��켣
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID);
        //��ȡ�¼�
        List<KoreographyEvent> rawEvents = rhythmTrack.GetAllEvents();
        for (int i = 0; i < rawEvents.Count; i++)
        {
            KoreographyEvent evt = rawEvents[i];
            int noteID = evt.GetIntValue();
            //������������
            for (int j = 0; j < noteLanes.Count; j++)
            {
                LineController line = noteLanes[j];
                if (noteID > 6)
                {
                    noteID = noteID - 6;
                    if (noteID > 6)
                    {
                        noteID = noteID - 6;
                    }
                }
                if (line.DoesMatch(noteID))
                {
                    line.AddEventToLane(evt);
                    break;
                }
            }
        }
        hitWindowRangeInSamples = (int)(SampleRate * hitWindowRangeInMS * 0.001f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPauseState)
        {
            return;
        }
        if (timeLeftToPlay > 0)
        {
            timeLeftToPlay -= Time.unscaledDeltaTime;
            if (timeLeftToPlay <= 0)
            {
                audioCom.Play();
                gameStart = true;
                timeLeftToPlay = 0;
            }
        }
        //��������ʱ��
        if (leadInTimeLeft > 0)
        {
            leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0);
        }
        if (gameStart)
        {
            if (!simpleMusicPlayer.IsPlaying)
            {
                gameOverUI.SetActive(true);
            }
        }

    }
    //��ʼ������ʱ��
    void InitializeLeadIn()
    {
        if (leadInTime > 0)
        {
            leadInTimeLeft = leadInTime;
            timeLeftToPlay = leadInTime;
        }
        else
        {
            audioCom.Play();
        }
    }
    //�ӳ���ȡ����ķ���
    public NoteObject GetFreshNoteObject()
    {
        NoteObject retObj;
        if (noteObjectPool.Count > 0)
        {
            retObj = noteObjectPool.Pop();
        }
        else
        {
            retObj = Instantiate<NoteObject>(noteObject);
        }
        //��ֹһ������������
        retObj.transform.position = Vector3.one*2;
        retObj.gameObject.SetActive(true);
        retObj.enabled = true;
        return retObj;
    }
    //����������Żض����
    public void ReturnNoteObjectToPool(NoteObject obj)
    {
        if (obj != null)
        {
            obj.enabled = false;
            obj.gameObject.SetActive(false);
            noteObjectPool.Push(obj);
        }
    }
    public GameObject GetFreshEffectObject(Stack<GameObject> stack,GameObject effectObject)
    {
        GameObject effectGo;
        if (stack.Count>0)
        {
            effectGo = stack.Pop();

        }
        else
        {
            effectGo = Instantiate(effectObject);
        }
        effectGo.SetActive(true);
        return effectGo;
    }
    public void ReturnEffectGoToPool(GameObject effectGo,Stack<GameObject> stack)
    {
        if (effectGo!=null)
        {
            effectGo.gameObject.SetActive(false);
            stack.Push(effectGo);
        }
    }
    //���·����ķ���
    public void UpdateScoreText(int addNum)
    {
        score += addNum;
        scoreText.text = score.ToString();
    }
    //��ͣ����
    public void PauseMusic()
    {
        if (!gameStart)
        {
            return;
        }
        simpleMusicPlayer.Pause();
    }
    //��������
    public void PlayMusic()
    {
        if (!gameStart)
        {
            return;
        }
        simpleMusicPlayer.Play();
    }
    public void Replay()
    {
        SceneManager.LoadScene(1);
    }
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }
}
