using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using VSLangProj;

namespace DBMLUpdate
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            if(connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object []contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                string toolsMenuName = "Tools";

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                //Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "DBMLUpdate", "DBMLUpdate", "Executes the command for DBMLUpdate", true, 37, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if((command != null) && (toolsPopup != null))
                    {
                        command.AddControl(toolsPopup.CommandBar, 1);
                    }
                }
                catch(System.ArgumentException)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                }
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }
        
        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if(commandName == "DBMLUpdate.Connect.DBMLUpdate")
                {
                    if(_applicationObject.Solution.IsOpen)
                    {
                        if(_applicationObject.ActiveDocument != null && _applicationObject.ActiveDocument.FullName.EndsWith(".dbml", StringComparison.InvariantCultureIgnoreCase))
                            status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                        else
                            status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
                    }
                    else
                        status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
                    
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if(commandName == "DBMLUpdate.Connect.DBMLUpdate")
                {
                    // Some initial setup create an output pain and grab status bar
                    if(_output == null)
                        _output = _applicationObject.ToolWindows.OutputWindow.OutputWindowPanes.Add("DBML Update");
                    _output.Clear();
                    EnvDTE.StatusBar status = _applicationObject.StatusBar;
                    _output.Activate();

                    try
                    {
                        //Check where dealing with a dbml file
                        if(_applicationObject.ActiveDocument != null && _applicationObject.ActiveDocument.FullName.EndsWith(".dbml", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _output.OutputString("Beginning Processing...\r\n");

                            // Grab dbml and save it if its unsaved
                            Document actDoc = _applicationObject.ActiveDocument;
                            ProjectItem pi = actDoc.ProjectItem;
                            string projPath = pi.ContainingProject.FullName;
                            string docFileName = actDoc.FullName;
                            if(!actDoc.Saved)
                            {
                                status.Text = "Saving Linq-to-SQL desinger diagram...";
                                actDoc.Save(docFileName);
                            }

                            UpdateStatus(_output, status, "Parsing DBML file...");
                            // Real work begins in DBMLDoc
                            DBMLDoc doc = new DBMLDoc(docFileName, pi);
                            _output.OutputString(string.Format("Parsed {0} Tables\r\n", doc.TableCount));

                            UpdateStatus(_output, status, "Updating from Database...");
                            doc.DBUpdate(_output);

                            status.Text = "Closing the DBML designer...";
                            actDoc.Close(actDoc.Saved ? vsSaveChanges.vsSaveChangesNo : vsSaveChanges.vsSaveChangesYes);

                            UpdateStatus(_output, status, "Creating new DBML file...");
                            doc.CreateDBML(docFileName, _output);

                            status.Text = "Opening designer...";
                            Window win = OpenDesigner(pi, docFileName);

                            UpdateStatus(_output, status, "Re-generating classes...");
                            try
                            {
                                ((VSProjectItem)win.ProjectItem.Object).RunCustomTool();
                            }
                            catch(Exception ex)
                            {
                                try
                                {
                                    win.Document.Save(docFileName);
                                }
                                catch
                                {
                                    UpdateStatus(_output, status, "Code re-generation failed: " + ex.Message);
                                }
                            }

                            _output.OutputString("Finished Processing\r\n");
                        }
                        else
                            _output.OutputString("The active document is not a Linq-to-SQL file.\r\n");
                    }
                    catch(Exception ex)
                    {
                        _output.OutputString("Refreshing entities failed due to exception:" + ex.ToString() + " Message:" + ex.Message + "\r\n");
                    }
                    finally
                    {
                        status.Text = string.Empty;
                        status.Progress(false, "", 0, 0);
                        if(_output != null)
                            _output.Activate();
                    }

                    handled = true;
                }
            }
        }

        private Window OpenDesigner(ProjectItem pi, string fileName)
        {
            Window win = null;
            try
            {
                win = pi.Open();
            }
            catch
            {
                win = _applicationObject.ItemOperations.OpenFile(fileName);
            }
            win.Activate();
            win.Visible = true;
            return win;
        }

        /// <summary>
        /// Updates the statusbar and output window with the same message
        /// </summary>
        /// <param name="output">The output pane</param>
        /// <param name="status">The status bar</param>
        /// <param name="text">The message to send to both</param>
        private void UpdateStatus(OutputWindowPane output, StatusBar status, string text)
        {
            status.Text = text;
            output.OutputString(text + "\r\n");
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private OutputWindowPane _output;
    }
}