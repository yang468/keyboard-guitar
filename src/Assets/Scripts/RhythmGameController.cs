using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;
using UnityEngine.UI;
using SonicBloom.Koreo.Players;
using UnityEngine.SceneManagement;
public class RhythmGameController : MonoBehaviour
{
    //用于目标生成的轨道的事件对应ID
    [EventID]
    public string eventID;
    public float noteSpeed = 1;     //音符速度
    [Tooltip("音符命中区间窗口")]
    [Range(8f, 300f)]
    public float hitWindowRangeInMS;    //检测音符击打区间
    //以Unity为单位访问当前命中窗口的大小
    public float WindowSizeInUnity
    {
        get
        {
            return noteSpeed * (hitWindowRangeInMS * 0.001f);
        }
    }
    //在音乐样本中的命中窗口
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
    //引用
    Koreography playingKoreo;
    public AudioSource audioCom;

    public List<LineController> noteLanes = new List<LineController>();

    SimpleMusicPlayer simpleMusicPlayer;
    public Transform simpleMusicPlayerTrans;
    public GameObject gameOverUIGo;
    //其他
    [Tooltip("开始播放音频之前提供的时间量，提前调用时间")]
    public float leadInTime;
    //音频播放之前的剩余时间量
    float leadInTimeLeft;
    //音乐开始之前的计时器
    float timeLeftToPlay;
    Stack<NoteObject> noteObjectPool = new Stack<NoteObject>();
    public Stack<GameObject> downEffectObjectPool = new Stack<GameObject>();
    public Stack<GameObject> hitEffectObjectPool = new Stack<GameObject>();
    public Stack<GameObject> hitLongNoteEffectObjectPool = new Stack<GameObject>();
    //预制体资源
    //音符
    public NoteObject noteObject;
    //按下特效
    public GameObject downEffectGo;
    //击中特效
    public GameObject hitEffectGo;
    //击中长音符特效
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
    //资源 更换音乐
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
        //获取事件轨迹
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID);
        //获取事件
        List<KoreographyEvent> rawEvents = rhythmTrack.GetAllEvents();
        for (int i = 0; i < rawEvents.Count; i++)
        {
            KoreographyEvent evt = rawEvents[i];
            int noteID = evt.GetIntValue();
            //遍历所有音轨
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
        //倒数引导时间
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
    //初始化引导时间
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
    //从池中取对象的方法
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
        //防止一闪而过的现象
        retObj.transform.position = Vector3.one*2;
        retObj.gameObject.SetActive(true);
        retObj.enabled = true;
        return retObj;
    }
    //将音符对象放回对象池
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
    //更新分数的方法
    public void UpdateScoreText(int addNum)
    {
        score += addNum;
        scoreText.text = score.ToString();
    }
    //暂停音乐
    public void PauseMusic()
    {
        if (!gameStart)
        {
            return;
        }
        simpleMusicPlayer.Pause();
    }
    //播放音乐
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
