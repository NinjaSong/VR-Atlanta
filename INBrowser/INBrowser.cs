/*************************************************************
 *       INBrowser (c) by CatBanana Studios 2016             *
 * 
 * Provides a simple interactive web view for Unity:
 * 
 *  - Pairs into Unity's Canvas system for easy integration via RawImage
 *  - Built on Chromium's foundation to provide a fully interactive user experience 
 *  - Supports HTML5 to provide end user with a richer experience on the web view
 * 
 * Usage (in Unity):
 *  - Create a new GameObject somewhere in your hierarchy
 *  - Add the INBrowser component to the new GameObject
 *  - Setup your Canvas UI as you see fit (note as of this
 *    first build, the RawImage you use must have a pivot
 *    of 0,0 to work properly.)
 *  - Drag the RawImage designated for the web view into the
 *    BrowserImage slot on the INBrowser component
 *  - Type in the initial url you want to load
 *
 * Usage (runtime):
 *  - Any changes to the BrowserImage are automatically passed
 *    to INBrowser.
 *  - Any changes to the URL are automatically passed to
 *    INBrowser.
 *  - Custom HTML can be passed to the OnLoadHTML(string) call
 *    which can be used to show something like an RSS feed.
 ************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace INBrowser
{
    /// <summary>
    /// INBrowser allows the user to utilize a Unity Canvas-based RawImage as a 
    /// Chromium-powered web view.  Once a RawImage is attached to BrowserImage, 
    /// and a url is provided to Url, INBrowser handles all other functionality 
    /// automatically via its Update() call.
    /// 
    /// INBrowser also supports custom HTML.  The HTML must be formatted external 
    /// to the web view, but is passed in via the OnLoadHtml() call and renders to 
    /// the screen appropriately.
    /// </summary>
    public class INBrowser : MonoBehaviour
    {
        /// <summary>
        /// Required Unity Canvas-based RawImage.  This must be attached either
        /// within a defined UI element, or set at runtime before OnEnable is
        /// called.
        /// </summary>
        public RawImage BrowserImage;

        /// <summary>
        /// Required string for current Url.  This must be set before OnEnable
        /// is called.
        /// </summary>
        public string Url;

        /// <summary>
        /// This interfaces with the background app process
        /// allowing Chromium to pair properly with Unity.
        /// </summary>
        private INBrowserConnector.INBrowserConnector _connectorInterface;

        /// <summary>
        /// This is set when the system has properly initialized the background 
        /// app process via _connectorInterface.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// This captures the Application's run in background setting prior to
        /// setting it to true for browser usage.
        /// </summary>
        private bool _runInBackground;

        /// <summary>
        /// This captures the Application's fixed time step prior to setting it
        /// for browser usage.
        /// </summary>
        private float _fixedTimeStep;
        /// <summary>
        /// This is used to compare versus the public Url variable.  If any changes occur,
        /// a load url signal is sent to the background app.
        /// </summary>
        private string _url;

        /// <summary>
        /// This is used to compare versus the BrowserImage's window position.  If any changes
        /// occur, a resize signal is sent to the background app.
        /// </summary>
        private float _x = 0;

        /// <summary>
        /// This is used to compare versus the BrowserImage's window position.  If any changes
        /// occur, a resize signal is sent to the background app.
        /// </summary>
        private float _y = 0;

        /// <summary>
        /// This is used to compare versus the Browser Image's window bounds.  If any changes
        /// occur, a resize signal is sent to the background app.
        /// </summary>
        private float _width = 0;

        /// <summary>
        /// This is used to compare versus the Browser Image's window bounds.  If any changes
        /// occur, a resize signal is sent to the background app.
        /// </summary>
        private float _height = 0;

        /// <summary>
        /// This is used to determine if the mouse is within the BrowserImage area.  
        /// If it is, a mouse move signal is sent to the background app.  If not, a mouse leave
        /// signal is sent to the background app.
        /// </summary>
        private bool _mouseEnteredArea = false;

        /// <summary>
        /// This is used to determine if a key is down.  This is required to provide a
        /// buffer for all input being fed to the background app.
        /// </summary>
        private bool _keyDown = false;

        /// <summary>
        /// This is used to determine if a key is stilldown.  This is required to provide
        /// a buffer for all input being fed to the background app.
        /// </summary>
        private bool _keyStillDown = false;

        /// <summary>
        /// This is used to time a key down event.
        /// </summary>
        private Stopwatch _inputTimer = new Stopwatch();

        /// <summary>
        /// This is used when a key is down.  The timer will increase until max timer,
        /// which will then allow the defined input to be fired off.
        /// </summary>
        private double _keyTimer = 0.0;

        /// <summary>
        /// This is used when a key is down. It is the initial max timer before the key
        /// timer resets when only _keyDown is set.
        /// </summary>
        private double _initialMaxTimer = 0.25;

        /// <summary>
        /// This is used when a key is down. It is the max timer before the key timer
        /// resets when both _keyDown and _keyStillDown are set.
        /// </summary>
        private double _maxTimer = 0.025;

        /// <summary>
        /// When Unity enables the GameObject containing INBrowser:
        /// 
        /// - Save the application's run in background setting.
        /// - Set the run in background setting to true.
        /// - Create the browser connector interface which also runs the background app.
        /// </summary>
        private void OnEnable()
        {
            _runInBackground = Application.runInBackground;
            Application.runInBackground = true;

            _fixedTimeStep = Time.fixedDeltaTime;
            Time.fixedDeltaTime = 0.0166f;

            _connectorInterface = new INBrowserConnector.INBrowserConnector(Application.isEditor, Application.dataPath);
        }

        /// <summary>
        /// When Unity calls FixedUpdate on the INBrowser GameObject,
        /// grab the latest render frame buffer and update the texture
        /// on the BrowserImage.
        /// </summary>
        private void FixedUpdate()
        {
            HandleRenderBuffer();
        }

        /// <summary>
        /// When Unity calls Update on the INBrowser GameObject, handle:
        /// 
        /// - Initialization of the background app.
        /// - Drawing of the latest render buffer for the attached BrowserImage.
        /// - Testing if the url has changed.
        /// - Testing if the BrowserImage has been resized.
        /// - Testing for keyboard input.
        /// - Testing for mouse input.
        /// </summary>
        private void Update()
        {
			//Url = transform.GetComponent<Text> ().text;
            HandleInitialize();
            HandleUrlChange();
            HandleResize();
            HandleKeyboardInput();
            HandleMouseInput();
        }

        /// <summary>
        /// When focus state changes in the application, send focus state 
        /// change signal to background app.
        /// </summary>
        /// <param name="focusStatus"></param>
        private void OnApplicationFocus(bool focusStatus)
        {
            _connectorInterface.OnFocusStateChange(focusStatus);
        }

        /// <summary>
        /// When custom HTML is passed into INBrowser, send load HTML
        /// signal to background app.
        /// </summary>
        /// <param name="html"></param>
        public void OnLoadHTML(string html)
        {
            _connectorInterface.OnLoadHTML(html);
        }

        /// <summary>
        /// When Unity disables the GameObject containing INBrowser:
        /// 
        /// - Shutdown the connector interface which gracefully shuts down
        ///   the background app.
        /// - Reset run in background to its original setting.
        /// </summary>
        private void OnDisable()
        {
            _connectorInterface.ShutdownConnector();
            Application.runInBackground = _runInBackground;
            Time.fixedDeltaTime = _fixedTimeStep;
        }

        /// <summary>
        /// Handle initialization if it has not been completed already and the
        /// BrowserImage is properly setup.  Set all private variables for
        /// future testing, and then pass the url, window position, and window
        /// bounds to the background app.
        /// </summary>
        private void HandleInitialize()
        {
            if (!_initialized && BrowserImage.rectTransform.rect.width != 0)
            {
                _url = Url;
                _x = BrowserImage.rectTransform.rect.x;
                _y = BrowserImage.rectTransform.rect.y;
                _width = BrowserImage.rectTransform.rect.width;
                _height = BrowserImage.rectTransform.rect.height;
                _connectorInterface.OnInitialize(_url, _x, _y, _width, _height);
                _initialized = true;
            }
            else if (!_initialized)
                return;
        }

        /// <summary>
        /// Grab the latest frame from the background app to render onto the
        /// BrowserImage.
        /// </summary>
        private void HandleRenderBuffer()
        {
            if (_connectorInterface.IsBufferReady())
            {
                if (BrowserImage.texture == null) BrowserImage.texture = new Texture2D((int)_width, (int)_height, TextureFormat.ARGB32, false, false);
                var texture = BrowserImage.texture as Texture2D;
                texture.LoadImage(_connectorInterface.GetBuffer());
                _connectorInterface.BufferHandled();
            }
        }

        /// <summary>
        /// Test if the Url has been changed.  If so, send a load url signal.
        /// </summary>
        private void HandleUrlChange()
        {
            if (!string.Equals(Url, _url))
            {
                _url = Url;
                _connectorInterface.OnLoadUrl(_url);
            }
        }

        /// <summary>
        /// Test if the BrowserImage has been resized.  If so, send a resize signal.
        /// </summary>
        private void HandleResize()
        {
            if (BrowserImage.rectTransform.rect.x != _x || BrowserImage.rectTransform.rect.y != _y ||
                BrowserImage.rectTransform.rect.width != _width || BrowserImage.rectTransform.rect.height != _height)
            {
                _x = BrowserImage.rectTransform.rect.x;
                _y = BrowserImage.rectTransform.rect.y;
                _width = BrowserImage.rectTransform.rect.width;
                _height = BrowserImage.rectTransform.rect.height;
                BrowserImage.texture = new Texture2D((int)_width, (int)_height, TextureFormat.ARGB32, false, false);
                _connectorInterface.OnResize(_x, _y, _width, _height);
            }
        }

        /// <summary>
        /// Test for keyboard input and send to background app.
        /// 
        /// - Test for characters first and send as a char down signal.
        /// - Test for non characters second and send as a key down signal.
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (!_keyDown)
            {
                _keyTimer = 0.0;

                if (Input.GetKeyDown(KeyCode.Backspace)) { KeyHelper("KeyPress", "Backspace"); return; }

                if (Input.inputString.Length > 0)
                {
                    _connectorInterface.OnCharDown(Input.inputString);
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Tab))  KeyHelper("KeyPress", "Tab");
                else if (Input.GetKeyDown(KeyCode.Delete)) KeyHelper("KeyPress", "Delete");
                else if (Input.GetKeyDown(KeyCode.PageUp)) KeyHelper("KeyPress", "PageUp");
                else if (Input.GetKeyDown(KeyCode.PageDown)) KeyHelper("KeyPress", "PageDown");
                else if (Input.GetKeyDown(KeyCode.Home)) KeyHelper("KeyPress", "Home");
                else if (Input.GetKeyDown(KeyCode.End)) KeyHelper("KeyPress", "End");
                else if (Input.GetKeyDown(KeyCode.UpArrow)) KeyHelper("KeyPress", "UpArrow");
                else if (Input.GetKeyDown(KeyCode.DownArrow)) KeyHelper("KeyPress", "DownArrow");
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) KeyHelper("KeyPress", "LeftArrow");
                else if (Input.GetKeyDown(KeyCode.RightArrow)) KeyHelper("KeyPress", "RightArrow");
                else
                {
                    _keyDown = false;
                    _keyStillDown = false;
                    _keyTimer = 0.0f;
                    _inputTimer.Stop();
                }
            }
            else
            {
                if (_keyTimer < ((_keyDown && _keyStillDown) ? _maxTimer : _initialMaxTimer))
                {
                    _inputTimer.Stop();
                    _keyTimer += _inputTimer.Elapsed.TotalMilliseconds * 0.001;
                    _inputTimer.Reset();
                    _inputTimer.Start();
                }

                if (Input.GetKey(KeyCode.Backspace)) { KeyHelper("KeyDown", "Backspace"); return; }

                if (Input.inputString.Length > 0)
                {
                    _connectorInterface.OnCharDown(Input.inputString);
                    _keyDown = false;
                    _keyStillDown = false;
                    _keyTimer = 0.0f;
                    _inputTimer.Stop();
                    return;
                }

                if (Input.GetKey(KeyCode.Tab)) KeyHelper("KeyDown", "Tab");
                else if (Input.GetKey(KeyCode.Delete)) KeyHelper("KeyDown", "Delete");
                else if (Input.GetKey(KeyCode.PageUp)) KeyHelper("KeyDown", "PageUp");
                else if (Input.GetKey(KeyCode.PageDown)) KeyHelper("KeyDown", "PageDown");
                else if (Input.GetKey(KeyCode.Home)) KeyHelper("KeyDown", "Home");
                else if (Input.GetKey(KeyCode.End)) KeyHelper("KeyDown", "End");
                else if (Input.GetKey(KeyCode.UpArrow)) KeyHelper("KeyDown", "UpArrow");
                else if (Input.GetKey(KeyCode.DownArrow)) KeyHelper("KeyDown", "DownArrow");
                else if (Input.GetKey(KeyCode.LeftArrow)) KeyHelper("KeyDown", "LeftArrow");
                else if (Input.GetKey(KeyCode.RightArrow)) KeyHelper("KeyDown", "RightArrow");
                else
                {
                    _keyDown = false;
                    _keyStillDown = false;
                    _keyTimer = 0.0f;
                    _inputTimer.Stop();
                }
            }
        }

        private void KeyHelper(string keyStatus, string keyString)
        {
            switch (keyStatus)
            {
                case "KeyPress":
                    {
                        _keyDown = true;
                        _inputTimer.Start();
                        _connectorInterface.OnKeyDown(keyString);
                        break;
                    }
                case "KeyDown":
                    {
                        _keyDown = true;

                        if (_keyTimer >= ((_keyDown && _keyStillDown) ? _maxTimer : _initialMaxTimer))
                        {
                            if (_keyTimer >= _initialMaxTimer) _keyStillDown = true;
                            _connectorInterface.OnKeyDown(keyString);
                            _keyTimer = 0.0f;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Test for mouse input and send to background app.
        /// 
        /// - Convert current mouse position into a pointer event.
        /// - Determine if the mouse is within the BrowserImage.
        /// - If not, and we were previously in the area, send mouse leave signal.
        /// - If so, convert mouse position to local space.
        /// - If mouse wheel is being used, send mouse wheel signal.
        /// - Send a mouse move signal.
        /// - If mouse button is down, send mouse down signal.
        /// - If mouse button is up, send mouse up signal. 
        /// </summary>
        private void HandleMouseInput()
        {
            var pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, hits);

            if (hits.Count <= 0)
            {
                if (_mouseEnteredArea)
                {
                    _connectorInterface.OnMouseLeave();
                    _mouseEnteredArea = false;
                }
                return;
            }

            foreach (var hit in hits.Where(hit => hit.gameObject.GetComponent<RawImage>() != null))
            {
                if (hit.gameObject != BrowserImage.gameObject) continue;
                _mouseEnteredArea = true;

                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(hit.gameObject.GetComponent<RawImage>().rectTransform,
                    pointerEventData.position,
                    null,
                    out localPosition);

                if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                {
                    _connectorInterface.OnMouseWheel(localPosition.x, _height - localPosition.y, Input.GetAxis("Mouse ScrollWheel"));
                }

                _connectorInterface.OnMouseMove(localPosition.x, _height - localPosition.y);

                if (Input.GetMouseButtonDown(0))
                {
                    _connectorInterface.OnMouseDown(localPosition.x, _height - localPosition.y, 0);
                }
                else if (Input.GetMouseButtonDown(2))
                {
                    _connectorInterface.OnMouseDown(localPosition.x, _height - localPosition.y, 1);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    _connectorInterface.OnMouseDown(localPosition.x, _height - localPosition.y, 2);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _connectorInterface.OnMouseUp(localPosition.x, _height - localPosition.y, 0);
                }
                else if (Input.GetMouseButtonUp(2))
                {
                    _connectorInterface.OnMouseUp(localPosition.x, _height - localPosition.y, 1);
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    _connectorInterface.OnMouseUp(localPosition.x, _height - localPosition.y, 2);
                }

                return;
            }

            if (_mouseEnteredArea)
            {
                _connectorInterface.OnMouseLeave();
                _mouseEnteredArea = false;
            }
        }
    }
}