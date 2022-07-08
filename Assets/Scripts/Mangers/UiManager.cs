using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MHamidi;
using MHamidi.Helper;
using MHamidi.UI.UI_Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Singlton;

public class UiManager : MonoBehaviour
{
    public static event Action ShowModal;
    public static event Action<string, string> ShowModalString;
    public static event Action<ModalWindowData> ShowModalData;
    
    //-------------------
    [Header("--------------------------------PlayerInputParent---")] [SerializeField]
    private RectTransform playerInputParent;
    [Header("--------------------------------BufferParent---")] [SerializeField]
    private RectTransform BufferParrent;

    private RectTransform mainBuffer;
    private RectTransform P1Buffer;
    private RectTransform P2Buffer;

    private List<GameObject> mainBufferButtons=new List<GameObject>();
    private List<GameObject> p1BufferButtons=new List<GameObject>();
    private List<GameObject> p2BufferButtons=new List<GameObject>();
    
    [Header("--------------------------------PlayButton---")]
    [SerializeField] private Button playButtton;
    [SerializeField] private TextMeshProUGUI playText;
    //------------ 
    private void OnEnable()
    {
        PhysicalInputManager.QuitApplication += ShowQuitingDialogue;
        GameManger.UpdatePlayerInput += ShowPlayrInput;
        GameManger.UpdateBufferUi += ShowBufferUi;
        CommandManger.UpdatePlay += UpdatePlayButton;
        CommandManger.ChangePlayButtonInteractivityStatus += ChangePlayButtonInterActivityStatus;
        CommandManger.AddToBuffer += AddToBufferUi;
        CommandManger.RemoveAtIdnexofBuffer += RemoveFromBufferUi;
    }

   


    private void OnDisable()
    {
        PhysicalInputManager.QuitApplication -= ShowQuitingDialogue;
        GameManger.UpdatePlayerInput -= ShowPlayrInput;
        CommandManger.UpdatePlay -= UpdatePlayButton;
        CommandManger.ChangePlayButtonInteractivityStatus -= ChangePlayButtonInterActivityStatus;
        CommandManger.AddToBuffer -= AddToBufferUi;
    }
    
    //-----------
    private void ShowQuitingDialogue()
    {
        ShowModalMessage(new ModalWindowData("Quit Game", "Are You Sure Want To Quit ?",
            " Yes!I Hate this Game", "No,My Mistake", new SlidInOut(),
            () => { Application.Quit(); }));
    }

    //------------
    
    private void ShowModalMessage()
    {
        ShowModal?.Invoke();
    }

    private void ShowModalMessage(string header, string message)
    {
        ShowModalString?.Invoke(header, message);
    }

    private void ShowModalMessage(ModalWindowData data)
    {
        ShowModalData?.Invoke(data);
    }

    private void ShowPlayrInput(List<int> avilableCommand)
    {
        ClearAllChildren();
        AddPlayerInput(avilableCommand);

        #region Functions

        

     
        void ClearAllChildren()
        {
            foreach (var item in playerInputParent.GetComponentsInChildren<GameButton>())
            {
                item.gameObject.SetActive(false);
                item.transform.SetParent(Pool.Instance.transform, false);
            }
        }
        void AddPlayerInput(List<int> avilableCommand)
        {
            foreach (var item in avilableCommand)
            {
                var buttonObject = Pool.Instance.Get("GameButton");
                buttonObject.transform.SetParent(playerInputParent, false);
                buttonObject.SetActive(true);
                var command = CommandManger.current.commandLookUpTable[item];
                var button = buttonObject.GetComponent<GameButton>();
                button.SetListener(() => { CommandManger.current.AddToCurrentBuffer(command); });
                var icon = Resources.Load<Sprite>(command.name);
                button.SetIcon(icon);
                button.gameObject.name = command.name;
            }
        }
        #endregion
    }
    private void ShowBufferUi(int buffer, int p1, int p2)
    {
        var ActiveBuffers = BufferParrent.GetComponentsInChildren<BufferUI>();
        foreach (var item in ActiveBuffers)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(Pool.Instance.transform,false);
        }

        mainBuffer=CreatBuffer(buffer,"Main",0);
        P1Buffer=CreatBuffer(p1,"P1",1);
        P2Buffer=CreatBuffer(p2,"P2",2);
        
        RectTransform CreatBuffer(int buffer,string name,int commandBuffer)
        { if (buffer<=0)
            {
                Util.ShowMessag($"the size was 0");
                return null;
                
            }
            var bufferObject = Pool.Instance.Get("Buffer");
            bufferObject.SetActive(true);
           
            bufferObject.transform.SetParent(BufferParrent, false);
            var mainBuffer = bufferObject.GetComponent<BufferUI>();
            mainBuffer.SetText(name);
            mainBuffer.SetSize(buffer);
            mainBuffer.SetOnClick(() => { CommandManger.current.SetCurrentBuffer(commandBuffer); });
            return mainBuffer.transform.Find("Buffer").GetComponent<RectTransform>();
        }
    }

    private void AddToBufferUi(int index, ICommand command)
    {
        if (index==0)
        {  var buttonObject = Pool.Instance.Get("GameButton");
            mainBufferButtons.Add(buttonObject);
            buttonObject.transform.SetParent(mainBuffer, false);
            buttonObject.SetActive(true);
            var button = buttonObject.GetComponent<GameButton>();
            button.SetListener(() => { CommandManger.current.RemoveFromBuffer(index,Math.Clamp(Dipendency.Instance.ComandManger.MainCommand.Count-1,0,8)); });
            var icon = Resources.Load<Sprite>(command.name);
            button.SetIcon(icon);
            button.gameObject.name = command.name;
            
        }
        else if (index==1)
        {
            var buttonObject = Pool.Instance.Get("GameButton");
            p1BufferButtons.Add(buttonObject);
            buttonObject.transform.SetParent(P1Buffer, false);
            buttonObject.SetActive(true);
            var button = buttonObject.GetComponent<GameButton>();
            button.SetListener(() => { CommandManger.current.RemoveFromBuffer(index,Math.Clamp(Dipendency.Instance.ComandManger.P1Command.Count-1,0,8)); });
            var icon = Resources.Load<Sprite>(command.name);
            button.SetIcon(icon);
            button.gameObject.name = command.name;
        }

        else if (index==2)
        {
            var buttonObject = Pool.Instance.Get("GameButton");
            p2BufferButtons.Add(buttonObject);
            buttonObject.transform.SetParent(P2Buffer, false);
            buttonObject.SetActive(true);
            var button = buttonObject.GetComponent<GameButton>();
            button.SetListener(() => { CommandManger.current.RemoveFromBuffer(index,Math.Clamp(Dipendency.Instance.ComandManger.p2Command.Count-1,0,8)); });
            var icon = Resources.Load<Sprite>(command.name);
            button.SetIcon(icon);
            button.gameObject.name = command.name;
        }
      
    }

    private void RemoveFromBufferUi(int index, int buttonindex)
    {
        if (index==0)
        {  
            mainBufferButtons[buttonindex].SetActive(false);
            mainBufferButtons.RemoveAt(buttonindex);
         
            
        }
        else if (index==1)
        {
          
            p1BufferButtons[buttonindex].SetActive(false);
            p1BufferButtons.RemoveAt(buttonindex);
           
        }

        else if (index==2)
        {
          
            p2BufferButtons[buttonindex].SetActive(false);
            p2BufferButtons.RemoveAt(buttonindex);
          
        }
    }
    private void UpdatePlayButton(Action onClick,string text)
    {
        playButtton.onClick.RemoveAllListeners();
        playButtton.onClick.AddListener(() => { onClick?.Invoke();});
        playText.text = text;
    }

    private void ChangePlayButtonInterActivityStatus()
    {
        playButtton.interactable = !playButtton.interactable;
    }

}