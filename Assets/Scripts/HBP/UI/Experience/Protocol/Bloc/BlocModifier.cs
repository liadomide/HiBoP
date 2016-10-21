﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using d = HBP.Data.Experience.Protocol;
using Tools.Unity;

namespace HBP.UI.Experience.Protocol
{
	public class BlocModifier : ItemModifier<d.Bloc> 
	{
		#region Properties
		InputField nameInputField;
		FileSelector imageFileSelector;
		InputField sortInputField;
		InputField windowMinInputField;
		InputField windowMaxInputField;
        InputField baseLineMinInputField;
        InputField baseLineMaxInputField;
		InputField mainEventLabelInputField;
		InputField mainEventCodesInputField;
		EventList eventList;
        IconList iconList;
		Button saveButton;
		Button addEventButton;
		Button removeEventButton;
        Button addIconButton;
        Button removeIconButton;
        #endregion

        #region Public Methods
        public override void Save()
        {
            iconList.SaveAll();
            eventList.SaveAll();
            base.Save();
        }
        public void AddSecondaryEvent()
		{
            d.Event newEvent = new d.Event();
            ItemTemp.AddSecondaryEvent(newEvent);
            eventList.Add(newEvent);
        }
        public void RemoveSecondaryEvent()
		{
            d.Event[] eventsToRemove = eventList.GetObjectsSelected();
            ItemTemp.RemoveSecondaryEvent(eventsToRemove);
            eventList.Remove(eventsToRemove);
        }
        public void AddIcon()
        {
            d.Icon newIcon = new d.Icon();
            ItemTemp.Scenario.Add(newIcon);
            iconList.Add(newIcon);
        }
        public void RemoveIcon()
        {
            d.Icon[] iconsToRemove = iconList.GetObjectsSelected();
            ItemTemp.Scenario.Remove(iconsToRemove);
            iconList.Remove(iconsToRemove);
        }
        #endregion

        #region Private Methods
        protected override void SetFields(d.Bloc objectToDisplay)
        {
            nameInputField.text = objectToDisplay.DisplayInformations.Name;
            nameInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.Name = value);

            imageFileSelector.File = objectToDisplay.DisplayInformations.Image;
            imageFileSelector.onValueChanged.AddListener((value) => ItemTemp.DisplayInformations.Image = value);

            sortInputField.text = objectToDisplay.DisplayInformations.Sort;
            sortInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.Sort = value);

            windowMinInputField.text = objectToDisplay.DisplayInformations.Window.x.ToString();
            windowMinInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.Window = new Vector2(float.Parse(value), ItemTemp.DisplayInformations.Window.y));

            windowMaxInputField.text = objectToDisplay.DisplayInformations.Window.y.ToString();
            windowMaxInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.Window = new Vector2(ItemTemp.DisplayInformations.Window.x, float.Parse(value)));

            baseLineMinInputField.text = objectToDisplay.DisplayInformations.BaseLine.x.ToString();
            baseLineMinInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.BaseLine = new Vector2(float.Parse(value),ItemTemp.DisplayInformations.BaseLine.y));

            baseLineMaxInputField.text = objectToDisplay.DisplayInformations.BaseLine.y.ToString();
            baseLineMaxInputField.onEndEdit.AddListener((value) => ItemTemp.DisplayInformations.BaseLine = new Vector2(ItemTemp.DisplayInformations.BaseLine.x, float.Parse(value)));

            mainEventLabelInputField.text = objectToDisplay.MainEvent.Name;
            mainEventLabelInputField.onEndEdit.AddListener((value) => ItemTemp.MainEvent.Name = value);

            mainEventCodesInputField.text = objectToDisplay.MainEvent.CodesString;
            mainEventCodesInputField.onEndEdit.AddListener((value) => ItemTemp.MainEvent.CodesString = value);

            eventList.Display(ItemTemp.SecondaryEvents.ToArray());
            iconList.Display(ItemTemp.Scenario.Icons.ToArray());
        }
        protected override void SetWindow()
        {
            nameInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("Name").FindChild("InputField").GetComponent<InputField>();
            imageFileSelector = transform.FindChild("Content").FindChild("Display informations").FindChild("Image").FindChild("Illustration").FindChild("FileSelector").GetComponent<FileSelector>();
            sortInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("Sort").FindChild("InputField").GetComponent<InputField>();
            windowMinInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("Window").FindChild("Panel").FindChild("Min").FindChild("InputField").GetComponent<InputField>();
            windowMaxInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("Window").FindChild("Panel").FindChild("Max").FindChild("InputField").GetComponent<InputField>();
            baseLineMinInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("BaseLine").FindChild("Panel").FindChild("Min").FindChild("InputField").GetComponent<InputField>();
            baseLineMaxInputField = transform.FindChild("Content").FindChild("Display informations").FindChild("BaseLine").FindChild("Panel").FindChild("Max").FindChild("InputField").GetComponent<InputField>();
            mainEventLabelInputField = transform.FindChild("Content").FindChild("Main Event").FindChild("Label").FindChild("InputField").GetComponent<InputField>();
            mainEventCodesInputField = transform.FindChild("Content").FindChild("Main Event").FindChild("Code").FindChild("InputField").GetComponent<InputField>();
            eventList = transform.FindChild("Content").FindChild("Secondary Events").FindChild("List").FindChild("List").FindChild("Viewport").FindChild("Content").GetComponent<EventList>();
            iconList = transform.FindChild("Content").FindChild("Iconic Scenario").FindChild("List").FindChild("List").FindChild("Viewport").FindChild("Content").GetComponent<IconList>();
            saveButton = transform.FindChild("Content").FindChild("Buttons").FindChild("Save").GetComponent<Button>();
            addEventButton = transform.FindChild("Content").FindChild("Secondary Events").FindChild("List").FindChild("Buttons").FindChild("Add").GetComponent<Button>();
            removeEventButton = transform.FindChild("Content").FindChild("Secondary Events").FindChild("List").FindChild("Buttons").FindChild("Remove").GetComponent<Button>();
            addIconButton = transform.FindChild("Content").FindChild("Iconic Scenario").FindChild("List").FindChild("Buttons").FindChild("Add").GetComponent<Button>();
            removeIconButton = transform.FindChild("Content").FindChild("Iconic Scenario").FindChild("List").FindChild("Buttons").FindChild("Remove").GetComponent<Button>();
        }
        protected override void SetInteractableFields(bool interactable)
        {
            nameInputField.interactable = interactable;
            imageFileSelector.interactable = interactable;
            sortInputField.interactable = interactable;
            windowMinInputField.interactable = interactable;
            windowMaxInputField.interactable = interactable;
            baseLineMinInputField.interactable = interactable;
            baseLineMaxInputField.interactable = interactable;
            mainEventLabelInputField.interactable = interactable;
            mainEventCodesInputField.interactable = interactable;
            saveButton.interactable = interactable;
            addEventButton.interactable = interactable;
            removeEventButton.interactable = interactable;
            addIconButton.interactable = interactable;
            removeIconButton.interactable = interactable;
        }
        #endregion
    }
}