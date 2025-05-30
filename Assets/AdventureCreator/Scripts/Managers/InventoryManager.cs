/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"InventoryManager.cs"
 * 
 *	This script handles the "Inventory" tab of the main wizard.
 *	Inventory items are defined with this.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	/**
	 * Handles the "Inventory" tab of the Game Editor window.
	 * All inventory items, inventory categories and recipes are defined here.
	 */
	[System.Serializable]
	public class InventoryManager : ScriptableObject
	{
		
		/** The game's full list of inventory items */
		public List<InvItem> items = new List<InvItem>();
		/** The game's full list of inventory item categories */
		public List<InvBin> bins = new List<InvBin>();
		/** The game's full list of inventory item properties */
		public List<InvVar> invVars = new List<InvVar>();
		/** The default ActionListAsset to run if an inventory combination is unhandled */
		public ActionListAsset unhandledCombine;
		/** The default ActionListAsset to run if using an inventory item on a Hotspot is unhandled */
		public ActionListAsset unhandledHotspot;
		/** If True, the Hotspot clicked on to initiate unhandledHotspot will be sent as a parameter to the ActionListAsset */
		public bool passUnhandledHotspotAsParameter;
		/** The default ActionListAsset to run if giving an inventory item to an NPC is unhandled */
		public ActionListAsset unhandledGive;
		/** The game's full list of available recipes */
		public List<Recipe> recipes = new List<Recipe>();
		/** The game's full list of documents */
		public List<Document> documents = new List<Document>();
		/** The game's full list of objectives */
		public List<Objective> objectives = new List<Objective>();

		public ObjectiveStateLabel objectiveStateActiveLabel = new ObjectiveStateLabel ("Active");
		public ObjectiveStateLabel objectiveStateCompleteLabel = new ObjectiveStateLabel ("Complete");
		public ObjectiveStateLabel objectiveStateFailLabel = new ObjectiveStateLabel ("Fail");
		
		
		#if UNITY_EDITOR

		private string nameFilter = "";
		private int categoryFilter = 0;
		private bool filterOnStart = false;
		private ObjectivesFilter objectivesFilter = ObjectivesFilter.Title;
		private enum ObjectivesFilter { Title, Description };
		
		private InvItem selectedItem;
		private InvVar selectedInvVar;
		private Recipe selectedRecipe;
		private Ingredient selectedIngredient;
		private int sideIngredient = -1;
		private int sideItem = -1;
		
		private Vector2 scrollPos;
		private bool showItemsTab = true;
		private bool showBinsTab = false;
		private bool showCraftingTab = false;
		private bool showPropertiesTab = false;
		private bool showDocumentsTab = false;
		private bool showObjectivesTab = false;

		private bool showUnhandledEvents = true;
		private bool showItemList = true;
		private bool showItemProperties = true;

		private bool showCraftingList = true;
		private bool showCraftingProperties = true;
		private bool showCraftingIntegredients = true;
		private bool showCraftingIntegredientProperties = true;
		private bool showPropertiesList = true;
		private bool showPropertiesProperties = true;

		private InvItem lastDragItemOver;
		private int lastSwapIndex;
		private bool ignoreDrag;
		private const string DragItemKey = "AC.InventoryItems";

		private InvBin lastDragBinOver;
		private const string DragBinKey = "AC.InventoryCategories";

		private InvVar lastDragVarOver;
		private const string DragVarKey = "AC.InventoryProperties";

		private Recipe lastDragRecipeOver;
		private const string DragRecipeKey = "AC.InventoryRecipes";

		private Objective lastDragObjectiveOver;
		private const string DragObjectiveKey = "AC.InventoryObjectives";

		private Document lastDragDocumentOver;
		private const string DragDocumentKey = "AC.InventoryDocuments";


		/** Shows the GUI. */
		public void ShowGUI (Rect windowRect, System.Action<ActionListAsset> showALAEditor)
		{
			EditorGUILayout.Space ();

			GUILayout.BeginHorizontal ();
			GUILayoutOption tabWidth = GUILayout.Width (windowRect.width / 3f - 5f); //GUILayout.MinWidth (60f);

			string label = (items.Count > 0) ? ("Items (" + items.Count + ")") : "Items";
			if (GUILayout.Toggle (showItemsTab, label, "toolbarbutton", tabWidth))
			{
				SetTab (0);
			}

			label = (bins.Count > 0) ? ("Categories (" + bins.Count + ")") : "Categories";
			if (GUILayout.Toggle (showBinsTab,  label, "toolbarbutton", tabWidth))
			{
				SetTab (1);
			}

			label = (invVars.Count > 0) ? ("Properties (" + invVars.Count + ")") : "Properties";
			if (GUILayout.Toggle (showPropertiesTab, label, "toolbarbutton", tabWidth))
			{
				SetTab (3);
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			label = (recipes.Count > 0) ? ("Crafting (" + recipes.Count + ")") : "Crafting";
			if (GUILayout.Toggle (showCraftingTab, label, "toolbarbutton", tabWidth))
			{
				SetTab (2);
			}

			label = (documents.Count > 0) ? ("Documents (" + documents.Count + ")") : "Documents";
			if (GUILayout.Toggle (showDocumentsTab, label, "toolbarbutton", tabWidth))
			{
				SetTab (4);
			}

			label = (objectives.Count > 0) ? ("Objectives (" + objectives.Count + ")") : "Objectives";
			if (GUILayout.Toggle (showObjectivesTab, label, "toolbarbutton", tabWidth))
			{
				SetTab (5);
			}

			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			
			if (showBinsTab)
			{
				BinsGUI ();
			}
			else if (showCraftingTab)
			{
				CraftingGUI (showALAEditor);
			}
			else if (showItemsTab)
			{
				ItemsGUI (showALAEditor);
			}
			else if (showPropertiesTab)
			{
				PropertiesGUI ();
			}
			else if (showDocumentsTab)
			{
				DocumentsGUI (windowRect.width);
			}
			else if (showObjectivesTab)
			{
				ObjectivesGUI (showALAEditor);
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void ItemsGUI (System.Action<ActionListAsset> showALAEditor)
		{
			showUnhandledEvents = CustomGUILayout.ToggleHeader (showUnhandledEvents, "Global unhandled events");
			if (showUnhandledEvents)
			{
				CustomGUILayout.BeginVertical ();
				unhandledCombine = ActionListAssetMenu.AssetGUI ("Combine:", unhandledCombine, "Inventory_Unhandled_Combine", "AC.KickStarter.runtimeInventory.unhandledCombine", "The default ActionList asset to run if an inventory combination is unhandled");
				unhandledHotspot = ActionListAssetMenu.AssetGUI ("Use on hotspot:", unhandledHotspot, "Inventory_Unhandled_Hotspot", "AC.KickStarter.runtimeInventory.unhandledHotspot", "The default ActionList asset to run if using an inventory item on a Hotspot is unhandled");
				if (KickStarter.settingsManager && KickStarter.settingsManager.CanGiveItems ())
				{
					unhandledGive = ActionListAssetMenu.AssetGUI ("Give:", unhandledGive, "Inventory_Unhandled_Give", "AC.KickStarter.runtimeInventory.unhandledGive", "The default ActionList asset to run if giving an inventory item to an NPC is unhandled ");
				}

				passUnhandledHotspotAsParameter = CustomGUILayout.ToggleLeft ("Pass Hotspot as GameObject parameter to unhandled interactions?", passUnhandledHotspotAsParameter, "AC.KickStarter.inventoryManager.passUnhandledHotspotAsParameter", "If True, the Hotspot clicked on to initiate unhandledHotspot will be sent as a parameter to the ActionList asset");
				if (passUnhandledHotspotAsParameter && unhandledHotspot != null)
				{
					EditorGUILayout.HelpBox ("The Hotspot will be set as " + unhandledHotspot.name + "'s first parameter, which must be set to type 'GameObject'.", MessageType.Info);
				}
				CustomGUILayout.EndVertical ();
			}

			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}

			EditorGUILayout.Space ();
			CreateItemsGUI (binList);
			EditorGUILayout.Space ();
			
			if (selectedItem != null && items.Contains (selectedItem))
			{
				string apiPrefix = "AC.KickStarter.runtimeInventory.GetItem (" + selectedItem.id + ")";

				showItemProperties = CustomGUILayout.ToggleHeader (showItemProperties, "Inventory item '" + selectedItem.label + "' settings");
				if (showItemProperties)
				{
					CustomGUILayout.BeginVertical ();
					selectedItem.ShowGUI (apiPrefix, showALAEditor);
					CustomGUILayout.EndVertical ();
				}
			}
		}


		// Categories

		private bool showCategoriesList = true;
		private bool showSelectedCategory = true;
		private InvBin selectedCategory;
		private int sideCategory = -1;


		private void BinsGUI ()
		{
			showCategoriesList = CustomGUILayout.ToggleHeader (showCategoriesList, "Categories");
			if (showCategoriesList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragBinKey, lastDragBinOver, lastDragBinOver != null ? lastDragBinOver.label : string.Empty, ref ignoreDrag, OnCompleteDragBin);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragBinOver = null;
					lastSwapIndex = -1;
				}

				CustomGUILayout.BeginScrollView (ref scrollPos, bins.Count);
				for (int i = 0; i < bins.Count; i++)
				{
					InvBin bin = bins[i];
					EditorGUILayout.BeginHorizontal ();

					if (GUILayout.Toggle (selectedCategory == bin, bin.EditorLabel, "Button"))
					{
						if (selectedCategory != bin)
						{
							DeactivateAllCategories ();
							ActivateCategory (bin);
						}
					}

					Rect buttonRect = GUILayoutUtility.GetLastRect ();
					if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
					{
						lastDragBinOver = bin;
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (bin);
					}
					
					EditorGUILayout.EndHorizontal ();

					if (IsDraggingBin ())
					{
						CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
					}
				}
				CustomGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Create new category"))
				{
					Undo.RecordObject (this, "Create new category");
					InvBin newBin = CreateNewCategory ();

					DeactivateAllCategories ();
					ActivateCategory (newBin);

					if (bins.Count == 1)
					{
						foreach (InvItem invItem in items)
						{
							if (invItem.binID == -1)
							{
								invItem.binID = bins[0].id;
							}
						}
					}
				}
				CustomGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();

			if (selectedCategory != null && bins.Contains (selectedCategory))
			{
				showSelectedCategory = CustomGUILayout.ToggleHeader (showSelectedCategory, "Category #" + selectedCategory.EditorLabel);
				if (showSelectedCategory)
				{
					CustomGUILayout.BeginVertical ();
					selectedCategory.label = CustomGUILayout.TextField ("Category name:", selectedCategory.label, "AC.KickStarter.inventoryManager.GetCategory (" + selectedCategory.id + ").label", "The category's editor name");
					selectedCategory.forItems = CustomGUILayout.Toggle ("Available to Items?", selectedCategory.forItems, "AC.KickStarter.inventoryManager.GetCategory (" + selectedCategory.id + ").forItems", "If True, the category is avaiable for Inventory items to use");
					selectedCategory.forDocuments = CustomGUILayout.Toggle ("Available to Documents?", selectedCategory.forDocuments, "AC.KickStarter.inventoryManager.GetCategory (" + selectedCategory.id + ").forDocuments", "If True, the category is avaiable for Documents to use");
					selectedCategory.forObjectives = CustomGUILayout.Toggle ("Available to Objectives?", selectedCategory.forObjectives, "AC.KickStarter.inventoryManager.GetCategory (" + selectedCategory.id + ").forObjectives", "If True, the category is avaiable for Objectives to use");
					CustomGUILayout.EndVertical ();
				}
			}
		}


		private void SideMenu (InvBin category)
		{
			GenericMenu menu = new GenericMenu ();
			sideCategory = bins.IndexOf (category);
			
			menu.AddItem (new GUIContent ("Delete"), false, CategoryCallback, "Delete");
			menu.ShowAsContext ();
		}


		private void CategoryCallback (object obj)
		{
			if (sideCategory >= 0)
			{
				InvBin tempCategory = bins[sideCategory];

				switch (obj.ToString ())
				{
					case "Delete":
						Undo.RecordObject (this, "Delete category");
						if (tempCategory == selectedCategory)
						{
							DeactivateAllCategories ();
						}
						bins.RemoveAt (sideCategory);
						break;

					default:
						break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideCategory = -1;
		}


		private void DeactivateAllCategories ()
		{
			selectedCategory = null;
		}


		private void ActivateCategory (InvBin category)
		{
			selectedCategory = category;
			EditorGUIUtility.editingTextField = false;
		}

		
		// Properties
		
		private void PropertiesGUI ()
		{
			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.EditorLabel);
			}
			
			CreatePropertiesGUI ();
			EditorGUILayout.Space ();
			
			if (selectedInvVar != null && invVars.Contains (selectedInvVar))
			{
				string apiPrefix = "AC.KickStarter.variablesManager.GetProperty (" + selectedInvVar.id + ")";

				showPropertiesProperties = CustomGUILayout.ToggleHeader (showPropertiesProperties, "Inventory property '" + selectedInvVar.label + "' properties");
				if (showPropertiesProperties)
				{
					CustomGUILayout.BeginVertical ();
					selectedInvVar.label = CustomGUILayout.TextField ("Name:", selectedInvVar.label, apiPrefix + ".label", "Its editor name");
					selectedInvVar.type = (VariableType) CustomGUILayout.EnumPopup ("Type:", selectedInvVar.type, apiPrefix + ".type", "Its variable type");

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Internal description:", GUILayout.MaxWidth (146f));
					selectedInvVar.description = EditorGUILayout.TextArea (selectedInvVar.description);
					EditorGUILayout.EndHorizontal ();

					if (selectedInvVar.type == VariableType.PopUp)
					{
						//selectedInvVar.popUps = VariablesManager.PopupsGUI (selectedInvVar.popUps);
						VariablesManager.ShowPopUpLabelsGUI (selectedInvVar, true);
					}
					
					EditorGUILayout.Space ();
					selectedInvVar.limitToCategories = EditorGUILayout.Toggle (new GUIContent ("Limit to set categories?", "If True, then the property will be limited to inventory items within certain categories"), selectedInvVar.limitToCategories);

					if (selectedInvVar.limitToCategories)
					{
						if (bins.Count > 0)
						{
							List<int> newCategoryIDs = new List<int>();
							foreach (InvBin bin in bins)
							{
								bool usesCategory = selectedInvVar.categoryIDs.Contains (bin.id);
								usesCategory = CustomGUILayout.ToggleLeft ("Use in '" + bin.label + "'?", usesCategory, apiPrefix + ".categoryIDs");
								if (usesCategory)
								{
									newCategoryIDs.Add (bin.id);
								}
							}
							selectedInvVar.categoryIDs = newCategoryIDs;
						}
						else
						{
							EditorGUILayout.HelpBox ("No categories are defined!", MessageType.Warning);
						}
					}
					CustomGUILayout.EndVertical ();
				}
			}
			
			if (GUI.changed)
			{
				foreach (InvItem item in items)
				{
					item.RebuildProperties ();
				}
				foreach (Document document in documents)
				{
					document.RebuildProperties ();
				}
				foreach (Objective objective in objectives)
				{
					objective.RebuildProperties ();
				}
			}
		}
		

		private void ResetFilter ()
		{
			nameFilter = string.Empty;
			categoryFilter = 0;
			filterOnStart = false;
			objectivesFilter = ObjectivesFilter.Title;
		}


		// Documents

		private bool showDocumentsList = true;
		private bool showSelectedDocument = true;
		private Document selectedDocument;
		private int sideDocument = -1;

		private void DocumentsGUI (float windowWidth)
		{
			showDocumentsList = CustomGUILayout.ToggleHeader (showDocumentsList, "Documents");
			if (showDocumentsList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragDocumentKey, lastDragDocumentOver, lastDragDocumentOver != null ? lastDragDocumentOver.title : string.Empty, ref ignoreDrag, OnCompleteDragDocument);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragDocumentOver = null;
					lastSwapIndex = -1;
				}

				CustomGUILayout.BeginScrollView (ref scrollPos, documents.Count);
				for (int i = 0; i < documents.Count; i++)
				{
					Document document = documents[i];

					EditorGUILayout.BeginHorizontal ();

					if (GUILayout.Toggle (selectedDocument == document, document.ID + ": " + document.title, "Button"))
					{
						if (selectedDocument != document)
						{
							DeactivateAllDocuments ();
							ActivateDocument (document);
						}
					}

					Rect buttonRect = GUILayoutUtility.GetLastRect ();
					if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
					{
						lastDragDocumentOver = document;
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (document);
					}
					
					EditorGUILayout.EndHorizontal ();

					if (IsDraggingDocument ())
					{
						CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
					}
				}
				CustomGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Create new Document"))
				{
					Undo.RecordObject (this, "Create new Document");

					if (documents.Count > 0)
					{
						List<int> idArray = new List<int>();
						foreach (Document document in documents)
						{
							idArray.Add (document.ID);
						}
						idArray.Sort ();

						Document newDocument = new Document (idArray.ToArray ());
						documents.Add (newDocument);

						DeactivateAllDocuments ();
						ActivateDocument (newDocument);
					}
					else
					{
						Document newDocument = new Document (0);
						documents.Add (newDocument);
						ActivateDocument (newDocument);
					}
				}
				CustomGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();

			if (selectedDocument != null && documents.Contains (selectedDocument))
			{
				showSelectedDocument = CustomGUILayout.ToggleHeader (showSelectedDocument, "Document #" + selectedDocument.ID + ": " + selectedDocument.Title);
				if (showSelectedDocument)
				{
					string apiPrefix = "AC.KickStarter.inventoryManager.GetDocument (" + selectedDocument.ID + ")";
					selectedDocument.ShowGUI (apiPrefix, bins, windowWidth);
				}
			}
		}


		private void SideMenu (Document document)
		{
			GenericMenu menu = new GenericMenu ();
			sideDocument = documents.IndexOf (document);
			
			menu.AddItem (new GUIContent ("Insert after"), false, DocumentCallback, "Insert after");
			if (documents.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, DocumentCallback, "Delete");
			}
			if (sideDocument > 0 || sideDocument < documents.Count-1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideDocument > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, DocumentCallback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, DocumentCallback, "Move up");
			}
			if (sideDocument < documents.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, DocumentCallback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, DocumentCallback, "Move to bottom");
			}

			menu.AddSeparator (string.Empty);
			menu.AddItem (new GUIContent ("Find references"), false, DocumentCallback, "Find references");
			menu.AddItem (new GUIContent ("Change ID"), false, DocumentCallback, "Change ID");
					
			if (Application.isPlaying)
			{
				menu.AddSeparator (string.Empty);
				if (KickStarter.runtimeDocuments.DocumentIsInCollection (document.ID))
				{
					menu.AddDisabledItem (new GUIContent ("Held by Player"));
				}
				else
				{
					menu.AddItem (new GUIContent ("Give to Player"), false, DocumentCallback, "Give to Player");
				}
			}

			menu.ShowAsContext ();
		}


		private void DocumentCallback (object obj)
		{
			if (sideDocument >= 0)
			{
				Document tempDocument = documents[sideDocument];

				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (this, "Insert Document");
						documents.Insert (sideDocument+1, new Document (GetDocumentIDList ().ToArray ()));
						break;
					
					case "Delete":
						Undo.RecordObject (this, "Delete Document");
						if (tempDocument == selectedDocument)
						{
							DeactivateAllDocuments ();
						}
						documents.RemoveAt (sideDocument);
						break;
					
					case "Move up":
						Undo.RecordObject (this, "Move Document up");
						documents.RemoveAt (sideDocument);
						documents.Insert (sideDocument-1, tempDocument);
						break;
					
					case "Move down":
						Undo.RecordObject (this, "Move Document down");
						documents.RemoveAt (sideDocument);
						documents.Insert (sideDocument+1, tempDocument);
						break;

					case "Move to top":
						Undo.RecordObject (this, "Move Document to top");
						documents.RemoveAt (sideDocument);
						documents.Insert (0, tempDocument);
						break;

					case "Move to bottom":
						Undo.RecordObject (this, "Move Document to bottom");
						documents.Add (tempDocument);
						documents.RemoveAt (sideDocument);
						break;

					case "Find references":
						FindReferences (tempDocument);
						break;

					case "Change ID":
						ReferenceUpdaterWindow.Init (ReferenceUpdaterWindow.ReferenceType.Document, tempDocument.Title, tempDocument.ID);
						break;

					case "Give to Player":
						KickStarter.runtimeDocuments.AddToCollection (tempDocument);
						ACDebug.Log ("Document " + tempDocument.ID.ToString () + " added to Player's inventory");
						break;

					default:
						break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideDocument = -1;
		}


		public static int DocumentSelectorList (int ID, string label = "Document:")
		{
			if (KickStarter.inventoryManager && KickStarter.inventoryManager.documents != null && KickStarter.inventoryManager.documents.Count > 0)
			{
				int tempNumber = -1;

				string[] labelList = new string[KickStarter.inventoryManager.documents.Count];
				for (int i=0; i<KickStarter.inventoryManager.documents.Count; i++)
				{
					labelList[i] = KickStarter.inventoryManager.documents[i].ID.ToString () + ": " + KickStarter.inventoryManager.documents[i].Title;

					if (KickStarter.inventoryManager.documents[i].ID == ID)
					{
						tempNumber = i;
					}
				}

				if (tempNumber == -1)
				{
					// Wasn't found (was deleted?), so revert to zero
					if (ID != 0)
						ACDebug.LogWarning ("Previously chosen Document no longer exists!");
					tempNumber = 0;
					ID = 0;
				}

				tempNumber = UnityEditor.EditorGUILayout.Popup (label, tempNumber, labelList);
				ID = KickStarter.inventoryManager.documents [tempNumber].ID;
			}
			else
			{
				UnityEditor.EditorGUILayout.HelpBox ("No Documents exist! They can be defined in the Inventory Manager.", UnityEditor.MessageType.Info);
				ID = 0;
			}

			return ID;
		}


		private void DeactivateAllDocuments ()
		{
			selectedDocument = null;
		}


		private List<int> GetDocumentIDList ()
		{
			List<int> idList = new List<int>();
			foreach (Document document in documents)
			{
				idList.Add (document.ID);
			}
			
			idList.Sort ();

			return idList;
		}


		private void ActivateDocument (Document document)
		{
			selectedDocument = document;
			EditorGUIUtility.editingTextField = false;
		}


		// Objectives

		private bool showObjectivesList = true;
		private bool showSelectedObjective = true;
		private Objective selectedObjective;
		private int sideObjective = -1;

		private void ObjectivesGUI (System.Action<ActionListAsset> showALAEditor)
		{
			showObjectivesList = CustomGUILayout.ToggleHeader (showObjectivesList, "Objectives");
			if (showObjectivesList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragObjectiveKey, lastDragObjectiveOver, lastDragObjectiveOver != null ? lastDragObjectiveOver.Title : string.Empty, ref ignoreDrag, OnCompleteDragObjective);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragObjectiveOver = null;
					lastSwapIndex = -1;
				}

				objectiveStateActiveLabel.label = CustomGUILayout.TextField ("'Active' state label:", objectiveStateActiveLabel.label, "AC.KickStarter.inventoryManager.objectiveStateActiveLabel");
				objectiveStateCompleteLabel.label = CustomGUILayout.TextField ("'Complete' state label:", objectiveStateCompleteLabel.label, "AC.KickStarter.inventoryManager.objectiveStateCompleteLabel");
				objectiveStateFailLabel.label = CustomGUILayout.TextField ("'Fail' state label:", objectiveStateFailLabel.label, "AC.KickStarter.inventoryManager.objectiobjectiveStateFailLabelveStateActiveLabel");

				EditorGUILayout.Space ();

				if (objectives != null && objectives.Count > 0)
				{
					objectivesFilter = (ObjectivesFilter) EditorGUILayout.EnumPopup ("Filter by:", objectivesFilter);
					nameFilter = EditorGUILayout.TextField (objectivesFilter.ToString () + " filter:", nameFilter);
					
					EditorGUILayout.Space ();
				}

				int numObjectivesInFilter = objectives.Count;
				if (!string.IsNullOrEmpty (nameFilter))
				{
					numObjectivesInFilter = 0;
					foreach (Objective objective in objectives)
					{
						switch (objectivesFilter)
						{
							case ObjectivesFilter.Title:
							default:
								if (objective.Title.ToLower ().Contains (nameFilter.ToLower ())) numObjectivesInFilter ++;
								break;

							case ObjectivesFilter.Description:
								if (objective.description.ToLower ().Contains (nameFilter.ToLower ())) numObjectivesInFilter ++;
								break;
						}
					}
				}

				if (numObjectivesInFilter > 0)
				{
					CustomGUILayout.BeginScrollView (ref scrollPos, numObjectivesInFilter);
					for (int i = 0; i < objectives.Count; i++)
					{
						Objective objective = objectives[i];

						if (!string.IsNullOrEmpty (nameFilter))
						{
							switch (objectivesFilter)
							{
								case ObjectivesFilter.Title:
								default:
									if (!objective.Title.ToLower ().Contains (nameFilter.ToLower ())) continue;
									break;

								case ObjectivesFilter.Description:
									if (!objective.description.ToLower ().Contains (nameFilter.ToLower ())) continue;
									break;
							}
						}

						EditorGUILayout.BeginHorizontal ();
					
						if (GUILayout.Toggle (selectedObjective == objective, objective.ID + ": " + objective.Title, "Button"))
						{
							if (selectedObjective != objective)
							{
								DeactivateAllObjectives ();
								ActivateObjective (objective);
							}
						}

						Rect buttonRect = GUILayoutUtility.GetLastRect ();
						if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
						{
							lastDragObjectiveOver = objective;
						}

						if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
						{
							SideMenu (objective);
						}
					
						EditorGUILayout.EndHorizontal ();

						if (IsDraggingObjective ())
						{
							CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
						}
					}
					CustomGUILayout.EndScrollView ();
				}

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Create new Objective"))
				{
					ResetFilter ();
					Undo.RecordObject (this, "Create new Objective");

					List<int> idArray = new List<int>();
					foreach (Objective objective in objectives)
					{
						idArray.Add (objective.ID);
					}
					idArray.Sort ();

					Objective newObjective = new Objective (idArray.ToArray ());
					objectives.Add (newObjective);

					DeactivateAllObjectives ();
					ActivateObjective (newObjective);
				}
				CustomGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();

			if (selectedObjective != null && objectives.Contains (selectedObjective))
			{
				showSelectedObjective = CustomGUILayout.ToggleHeader (showSelectedObjective, "Objective #" + selectedObjective.ID + ": " + selectedObjective.Title);
				if (showSelectedObjective)
				{
					string apiPrefix = "AC.KickStarter.inventoryManager.GetObjective (" + selectedObjective.ID + ")";
					selectedObjective.ShowGUI (apiPrefix, showALAEditor);
				}
			}
		}


		private void SideMenu (Objective objective)
		{
			GenericMenu menu = new GenericMenu ();
			sideObjective = objectives.IndexOf (objective);
			
			menu.AddItem (new GUIContent ("Insert after"), false, ObjectiveCallback, "Insert after");
			if (objectives.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, ObjectiveCallback, "Delete");
			}
			if (sideObjective > 0 || sideObjective < objectives.Count-1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideObjective > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, ObjectiveCallback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, ObjectiveCallback, "Move up");
			}
			if (sideObjective < objectives.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, ObjectiveCallback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, ObjectiveCallback, "Move to bottom");
			}

			menu.AddSeparator (string.Empty);
			menu.AddItem (new GUIContent ("Find references"), false, ObjectiveCallback, "Find references");
			menu.AddItem (new GUIContent ("Change ID"), false, ObjectiveCallback, "Change ID");

			menu.ShowAsContext ();
		}


		private void ObjectiveCallback (object obj)
		{
			if (sideObjective >= 0)
			{
				Objective tempObjective = objectives[sideObjective];
				ResetFilter ();

				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (this, "Insert Objective");
						List<int> idArray = new List<int>();
						foreach (Objective objective in objectives)
						{
							idArray.Add (objective.ID);
						}
						idArray.Sort ();
						objectives.Insert (sideObjective+1, new Objective (idArray.ToArray ()));
						break;
					
					case "Delete":
						Undo.RecordObject (this, "Delete Objective");
						if (tempObjective == selectedObjective)
						{
							DeactivateAllObjectives ();
						}
						objectives.RemoveAt (sideObjective);
						break;
					
					case "Move up":
						Undo.RecordObject (this, "Move Objective up");
						objectives.RemoveAt (sideObjective);
						objectives.Insert (sideObjective-1, tempObjective);
						break;
					
					case "Move down":
						Undo.RecordObject (this, "Move Objective down");
						objectives.RemoveAt (sideObjective);
						objectives.Insert (sideObjective+1, tempObjective);
						break;

					case "Move to top":
						Undo.RecordObject (this, "Move Objective to top");
						objectives.RemoveAt (sideObjective);
						objectives.Insert (0, tempObjective);
						break;

					case "Move to bottom":
						Undo.RecordObject (this, "Move Objective to bottom");
						objectives.Add (tempObjective);
						objectives.RemoveAt (sideObjective);
						break;

					case "Find references":
						FindReferences (tempObjective);
						break;

					case "Change ID":
						ReferenceUpdaterWindow.Init (ReferenceUpdaterWindow.ReferenceType.Objective, tempObjective.Title, tempObjective.ID);
						break;

					default:
						break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideObjective = -1;
		}


		public static int ObjectiveSelectorList (int ID, string label = "Objective:")
		{
			if (KickStarter.inventoryManager && KickStarter.inventoryManager.objectives != null && KickStarter.inventoryManager.objectives.Count > 0)
			{
				int tempNumber = -1;

				string[] labelList = new string[KickStarter.inventoryManager.objectives.Count];
				for (int i=0; i<KickStarter.inventoryManager.objectives.Count; i++)
				{
					labelList[i] = KickStarter.inventoryManager.objectives[i].Title;

					if (KickStarter.inventoryManager.objectives[i].ID == ID)
					{
						tempNumber = i;
					}
				}

				if (tempNumber == -1)
				{
					// Wasn't found (was deleted?), so revert to zero
					if (ID != 0)
						ACDebug.LogWarning ("Previously chosen Objective no longer exists!");
					tempNumber = 0;
					ID = 0;
				}

				tempNumber = UnityEditor.EditorGUILayout.Popup (label, tempNumber, labelList);
				ID = KickStarter.inventoryManager.objectives [tempNumber].ID;
			}
			else
			{
				UnityEditor.EditorGUILayout.HelpBox ("No Objectives exist! They can be defined in the Inventory Manager.", UnityEditor.MessageType.Info);
				ID = 0;
			}

			return ID;
		}


		private void DeactivateAllObjectives ()
		{
			selectedObjective = null;
		}


		private void ActivateObjective (Objective objective)
		{
			selectedObjective = objective;
			EditorGUIUtility.editingTextField = false;
		}


		// Items
		
		private void CreateItemsGUI (List<string> binList)
		{
			showItemList = CustomGUILayout.ToggleHeader (showItemList, "Inventory items");
			if (showItemList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragItemKey, lastDragItemOver, lastDragItemOver != null ? lastDragItemOver.label : string.Empty, ref ignoreDrag, OnCompleteDragItem);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragItemOver = null;
					lastSwapIndex = -1;
				}

				if (items != null && items.Count > 0)
				{
					nameFilter = EditorGUILayout.TextField ("Name filter:", nameFilter);
					if (binList != null && binList.Count > 0)
					{
						string[] binArray = new string[binList.Count+1];
						binArray[0] = "(Any)";
						for (int i=0; i<binList.Count; i++)
						{
							binArray[i+1] = binList[i];
						}

						categoryFilter = EditorGUILayout.Popup ("Category filter:", categoryFilter, binArray);
					}
					else
					{
						categoryFilter = 0;
					}
					filterOnStart = EditorGUILayout.Toggle ("Filter by 'Carry on start?'?", filterOnStart);

					EditorGUILayout.Space ();
				}

				int numInFilter = 0;
				foreach (InvItem item in items)
				{
					item.showInFilter = false;
					if (string.IsNullOrEmpty (nameFilter) || item.label.ToLower ().Contains (nameFilter.ToLower ()))
					{
						if (categoryFilter <= 0 || GetBinSlot (item.binID) == (categoryFilter-1))
						{
							if (!filterOnStart || item.carryOnStart)
							{
								item.showInFilter = true;
								numInFilter ++;
							}
						}
					}
				}

				if (numInFilter > 0)
				{
					CustomGUILayout.BeginScrollView (ref scrollPos, numInFilter);
					for (int i = 0; i < items.Count; i++)
					{
						InvItem item = items[i];
						if (!item.showInFilter) continue;

						EditorGUILayout.BeginHorizontal ();
						
						string buttonLabel = item.label;
						if (string.IsNullOrEmpty (buttonLabel))
						{
							buttonLabel = "(Untitled)";	
						}
						
						if (GUILayout.Toggle (selectedItem == item, item.id + ": " + buttonLabel, "Button"))
						{
							if (selectedItem != item)
							{
								DeactivateAllItems ();
								ActivateItem (item);
							}
						}

						Rect buttonRect = GUILayoutUtility.GetLastRect ();
						if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
						{
							lastDragItemOver = item;
						}

						if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
						{
							SideMenu (item);
						}
						
						EditorGUILayout.EndHorizontal ();

						if (IsDraggingItem ())
						{
							CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
						}
					}

					CustomGUILayout.EndScrollView ();
					if (numInFilter != items.Count)
					{
						EditorGUILayout.HelpBox ("Filtering " + numInFilter + " out of " + items.Count + " items.", MessageType.Info);
					}
				}
				else if (items.Count > 0)
				{
					EditorGUILayout.HelpBox ("No items that match the above filters have been created.", MessageType.Info);
				}

				EditorGUILayout.Space ();
				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Create new item"))
				{
					Undo.RecordObject (this, "Create inventory item");
					
					int newItemCategory = (categoryFilter > 0 && bins.Count >= (categoryFilter-1)) ? bins[categoryFilter-1].id : -1;
					if (newItemCategory < 0)
					{
						ResetFilter ();
					}
					InvItem newItem = CreateNewItem ();
					DeactivateAllItems ();
					ActivateItem (newItem);

					if (newItemCategory >= 0)
					{
						newItem.binID = newItemCategory;
					}
				}

				if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
				{
					ItemsSideMenu ();
				}
				EditorGUILayout.EndHorizontal ();
				CustomGUILayout.EndVertical ();
			}
		}


		private void ImportItems ()
		{
			bool canProceed = EditorUtility.DisplayDialog ("Import inventory items", "AC will now prompt you for a CSV or XML file to import. It is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			string fileName = EditorUtility.OpenFilePanel ("Import inventory item data", "Assets", "csv,xml");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (System.IO.File.Exists (fileName))
			{
				string fileText = Serializer.LoadFile (fileName);
				if (!string.IsNullOrEmpty (fileText))
				{
					if (fileName.EndsWith (".xml"))
					{
						string[,] xmlOutput = SML.SMLReader.SplitXMLGrid (fileText);
						InvItemImportWizardWindow.Init (this, xmlOutput);
					}
					else
					{
						string[,] csvOutput = CSVReader.SplitCsvGrid (fileText);
						InvItemImportWizardWindow.Init (this, csvOutput);
					}
				}
			}
		}


		/**
		 * <summary>Creates a new inventory item</summary>
		 * <param name = "newID">If >= 0, the ID number of the item, if available.  If it is already taken, the item will not be created</param>
		 * <returns>The newly-created item</returns>
		 */
		public InvItem CreateNewItem (int newID = -1)
		{
			List<int> idList = GetIDList ();
			if (newID >= 0)
			{
				if (idList.Contains (newID))
				{
					return null;
				}
				InvItem newItem = new InvItem (newID);
				items.Add (newItem);
				EditorUtility.SetDirty (this);
				return newItem;
			}
			else
			{
				InvItem newItem = new InvItem (idList.ToArray ());
				items.Add (newItem);
				EditorUtility.SetDirty (this);
				return newItem;
			}
		}
		

		/**
		 * <summary>Creates a new inventory item property</summary>
		 * <returns>The newly-created item property</returns>
		 */
		public InvVar CreateNewProperty ()
		{
			InvVar newInvVar = new InvVar (GetIDArrayProperty ());
			invVars.Add (newInvVar);
			EditorUtility.SetDirty (this);
			return newInvVar;
		}


		/**
		 * <summary>Creates a new inventory item category</summary>
		 * <returns>The newly-created item category</returns>
		 */
		public InvBin CreateNewCategory ()
		{
			List<int> idArray = new List<int>();
			foreach (InvBin bin in bins)
			{
				idArray.Add (bin.id);
			}
			idArray.Sort ();

			InvBin newBin = new InvBin (idArray.ToArray ());
			bins.Add (newBin);
			return newBin;
		}

		
		private void CreatePropertiesGUI ()
		{
			showPropertiesList = CustomGUILayout.ToggleHeader (showPropertiesList, "Inventory properties");
			if (showPropertiesList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragVarKey, lastDragVarOver, lastDragVarOver != null ? lastDragVarOver.label : string.Empty, ref ignoreDrag, OnCompleteDragVar);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragVarOver = null;
					lastSwapIndex = -1;
				}

				CustomGUILayout.BeginScrollView (ref scrollPos, invVars.Count);
				for (int i = 0; i < invVars.Count; i++)
				{
					InvVar invVar = invVars[i];
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = invVar.label;
					if (string.IsNullOrEmpty (buttonLabel))
					{
						buttonLabel = "(Untitled)";	
					}
					
					if (GUILayout.Toggle (selectedInvVar == invVar, invVar.id + ": " + buttonLabel, "Button"))
					{
						if (selectedInvVar != invVar)
						{
							DeactivateAllInvVars ();
							ActivateItem (invVar);
						}
					}

					Rect buttonRect = GUILayoutUtility.GetLastRect ();
					if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
					{
						lastDragVarOver = invVar;
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (invVar);
					}
					
					EditorGUILayout.EndHorizontal ();

					if (IsDraggingVar ())
					{
						CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
					}
				}
				CustomGUILayout.EndScrollView ();
				
				if (GUILayout.Button ("Create new property"))
				{
					Undo.RecordObject (this, "Create inventory property");
					
					InvVar newInvVar = CreateNewProperty ();
					DeactivateAllInvVars ();
					ActivateItem (newInvVar);
				}
				CustomGUILayout.EndVertical ();
			}
		}
		
		
		private void ActivateItem (InvItem item)
		{
			selectedItem = item;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void ActivateItem (InvVar invVar)
		{
			selectedInvVar = invVar;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void DeactivateAllItems ()
		{
			selectedItem = null;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void DeactivateAllInvVars ()
		{
			selectedInvVar = null;
		}
		
		
		private void ActivateRecipe (Recipe recipe)
		{
			selectedRecipe = recipe;
		}
		
		
		private void DeactivateAllRecipes ()
		{
			selectedRecipe = null;
			selectedIngredient = null;
		}


		private void ActivateIngredient (Ingredient ingredient)
		{
			selectedIngredient = ingredient;
		}


		private void DeactivateAllIngredients ()
		{
			selectedIngredient = null;
		}


		private void SideMenu (Ingredient ingredient)
		{
			if (selectedRecipe == null) return;

			GenericMenu menu = new GenericMenu ();
			sideIngredient = selectedRecipe.ingredients.IndexOf (ingredient);

			menu.AddItem (new GUIContent ("Delete"), false, IngredientCallback, "Delete");
			menu.ShowAsContext ();
		}


		private void IngredientCallback (object obj)
		{
			if (sideIngredient >= 0)
			{
				Ingredient tempIngredient = selectedRecipe.ingredients[sideIngredient];

				switch (obj.ToString ())
				{
					case "Delete":
						Undo.RecordObject (this, "Delete ingredient");
						if (tempIngredient == selectedIngredient)
						{
							DeactivateAllIngredients ();
						}
						selectedRecipe.ingredients.RemoveAt (sideIngredient);
						break;

					default:
						break;
				}
			}

			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();

			sideCategory = -1;
		}


		private void ItemsSideMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			
			menu.AddItem (new GUIContent ("Import items..."), false, ItemsCallback, "Import");
			menu.AddItem (new GUIContent ("Export items..."), false, ItemsCallback, "Export");

			if (items.Count > 0)
			{
				menu.AddSeparator (string.Empty);
				
				if (Application.isPlaying)
				{
					menu.AddItem (new GUIContent ("Give all to Player"), false, ItemsCallback, "Give all to Player");
				}
				else
				{
					menu.AddItem (new GUIContent ("Delete all"), false, ItemsCallback, "Delete all");
				}
			}


			menu.AddSeparator (string.Empty);
			menu.AddItem (new GUIContent ("Sort/By ID"), false, ItemsCallback, "SortByID");
			menu.AddItem (new GUIContent ("Sort/By name"), false, ItemsCallback, "SortByName");

			menu.ShowAsContext ();
		}


		private void ItemsCallback (object obj)
		{
			switch (obj.ToString ())
			{
				case "Import":
					ImportItems ();
					break;

				case "Export":
					InvItemExportWizardWindow.Init (this);
					break;

				case "Give all to Player":
					foreach (InvItem item in items)
					{
						KickStarter.runtimeInventory.PlayerInvCollection.AddToEnd (new InvInstance (item));
					}
					ACDebug.Log ("All items added to Player's inventory");
					break;

				case "Delete all":
					Undo.RecordObject (this, "Delete all items");
					items.Clear ();
					EditorUtility.SetDirty (this);
					AssetDatabase.SaveAssets ();
					break;

				case "SortByID":
					Undo.RecordObject (this, "Sort items by ID");
					items.Sort (delegate (InvItem i1, InvItem i2) { return i1.id.CompareTo (i2.id); });
					EditorUtility.SetDirty (this);
					AssetDatabase.SaveAssets ();
					break;

				case "SortByName":
					Undo.RecordObject (this, "Sort items by name");
					items.Sort (delegate (InvItem i1, InvItem i2) { return i1.label.CompareTo (i2.label); });
					EditorUtility.SetDirty (this);
					AssetDatabase.SaveAssets ();
					break;

				default:
					break;
			}
		}
		
		
		private void SideMenu (InvItem item)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = items.IndexOf (item);
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (items.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideItem > 0 || sideItem < items.Count-1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (sideItem < items.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
			}

			menu.AddSeparator (string.Empty);
			menu.AddItem (new GUIContent ("Find references"), false, Callback, "Find references");
			menu.AddItem (new GUIContent ("Change ID"), false, Callback, "Change ID");
			menu.AddItem (new GUIContent ("Create Recipe"), false, Callback, "Create Recipe");

			if (Application.isPlaying)
			{
				if (KickStarter.runtimeInventory.IsCarryingItem (item.id))
				{
					menu.AddDisabledItem (new GUIContent ("Held by Player"));
				}
				else
				{
					menu.AddItem (new GUIContent ("Give to Player"), false, Callback, "Give to Player");
				}
			}

			menu.ShowAsContext ();
		}


		private void SideMenu (InvVar invVar)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = invVars.IndexOf (invVar);
			
			menu.AddItem (new GUIContent ("Insert after"), false, PropertyCallback, "Insert after");
			if (invVars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, PropertyCallback, "Delete");
			}
			if (sideItem > 0 || sideItem < invVars.Count-1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, PropertyCallback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, PropertyCallback, "Move up");
			}
			if (sideItem < invVars.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, PropertyCallback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, PropertyCallback, "Move to bottom");
			}

			menu.ShowAsContext ();
		}


		private void SideMenu (Recipe recipe)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = recipes.IndexOf (recipe);
			
			menu.AddItem (new GUIContent ("Insert after"), false, RecipeCallback, "Insert after");
			if (recipes.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, RecipeCallback, "Delete");
			}
			if (sideItem > 0 || sideItem < recipes.Count-1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, RecipeCallback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, RecipeCallback, "Move up");
			}
			if (sideItem < recipes.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, RecipeCallback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, RecipeCallback, "Move to bottom");
			}

			menu.ShowAsContext ();
		}

		
		private void Callback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvItem tempItem = items[sideItem];
				
				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (this, "Insert item");
						items.Insert (sideItem+1, new InvItem (GetIDList ().ToArray ()));
						break;
					
					case "Delete":
						Undo.RecordObject (this, "Delete item");
						DeactivateAllItems ();
						items.RemoveAt (sideItem);
						break;
					
					case "Move up":
						Undo.RecordObject (this, "Move item up");
						items.RemoveAt (sideItem);
						items.Insert (sideItem-1, tempItem);
						break;
					
					case "Move down":
						Undo.RecordObject (this, "Move item down");
						items.RemoveAt (sideItem);
						items.Insert (sideItem+1, tempItem);
						break;

					case "Move to top":
						Undo.RecordObject (this, "Move item to top");
						items.RemoveAt (sideItem);
						items.Insert (0, tempItem);
						break;

					case "Move to bottom":
						Undo.RecordObject (this, "Move item to bottom");
						items.Add (tempItem);
						items.RemoveAt (sideItem);
						break;

					case "Find references":
						FindReferences (tempItem);
						break;

					case "Change ID":
						ReferenceUpdaterWindow.Init (ReferenceUpdaterWindow.ReferenceType.InventoryItem, tempItem.label, tempItem.id);
						break;

					case "Create Recipe":
						Recipe recipe = new Recipe (GetIDArrayRecipe ());
						recipe.resultID = tempItem.id;
						recipes.Add (recipe);
						SetTab (2);
						ActivateRecipe (recipe);
						break;

					case "Give to Player":
						KickStarter.runtimeInventory.PlayerInvCollection.AddToEnd (new InvInstance (tempItem.id));
						ACDebug.Log ("Item " + tempItem.label + " added to Player's inventory");
						break;

					default:
						break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}


		private void PropertyCallback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvVar tempVar = invVars[sideItem];
				
				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (this, "Insert property");
						invVars.Insert (sideItem+1, new InvVar (GetIDArrayProperty ()));
						break;
					
					case "Delete":
						Undo.RecordObject (this, "Delete property");
						DeactivateAllInvVars ();
						invVars.RemoveAt (sideItem);
						break;
					
					case "Move up":
						Undo.RecordObject (this, "Move property up");
						invVars.RemoveAt (sideItem);
						invVars.Insert (sideItem-1, tempVar);
						break;
					
					case "Move down":
						Undo.RecordObject (this, "Move property down");
						invVars.RemoveAt (sideItem);
						invVars.Insert (sideItem+1, tempVar);
						break;

					case "Move to top":
						Undo.RecordObject (this, "Move property to top");
						invVars.RemoveAt (sideItem);
						invVars.Insert (0, tempVar);
						break;

					case "Move to bottom":
						Undo.RecordObject (this, "Move property to bottom");
						invVars.Add (tempVar);
						invVars.RemoveAt (sideItem);
						break;

					default:
						break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}


		private void RecipeCallback (object obj)
		{
			if (sideItem >= 0)
			{
				Recipe tempRecipe = recipes[sideItem];
				
				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (this, "Insert recipe");
						recipes.Insert (sideItem+1, new Recipe (GetIDArrayRecipe ()));
						break;
					
					case "Delete":
						Undo.RecordObject (this, "Delete recipe");
						DeactivateAllRecipes ();
						recipes.RemoveAt (sideItem);
						break;
					
					case "Move up":
						Undo.RecordObject (this, "Move recipe up");
						recipes.RemoveAt (sideItem);
						recipes.Insert (sideItem-1, tempRecipe);
						break;
					
					case "Move down":
						Undo.RecordObject (this, "Move recipe down");
						recipes.RemoveAt (sideItem);
						recipes.Insert (sideItem+1, tempRecipe);
						break;

					case "Move to top":
						Undo.RecordObject (this, "Move recipe to top");
						recipes.RemoveAt (sideItem);
						recipes.Insert (0, tempRecipe);
						break;

					case "Move to bottom":
						Undo.RecordObject (this, "Move recipe to bottom");
						recipes.Add (tempRecipe);
						recipes.RemoveAt (sideItem);
						break;

					default:
						break;
				}
			}

			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}
		
		
		private void CraftingGUI (System.Action<ActionListAsset> showALAEditor)
		{
			showCraftingList = CustomGUILayout.ToggleHeader (showCraftingList, "Crafting recipes");
			if (showCraftingList)
			{
				CustomGUILayout.BeginVertical ();
				CustomGUILayout.UpdateDrag (DragRecipeKey, lastDragRecipeOver, lastDragRecipeOver != null ? lastDragRecipeOver.label : string.Empty, ref ignoreDrag, OnCompleteDragRecipe);
				if (Event.current.type == EventType.Repaint)
				{
					lastDragRecipeOver = null;
					lastSwapIndex = -1;
				}

				if (items.Count == 0)
				{
					EditorGUILayout.HelpBox ("No inventory items defined!", MessageType.Info);
					CustomGUILayout.EndVertical ();
					return;
				}

				if (recipes != null && recipes.Count > 0)
				{
					nameFilter = EditorGUILayout.TextField ("Name filter:", nameFilter);
					
					List<string> binList = new List<string>();
					foreach (InvBin bin in bins)
					{
						binList.Add (bin.label);
					}
			
					if (binList != null && binList.Count > 0)
					{
						string[] binArray = new string[binList.Count+1];
						binArray[0] = "(Any)";
						for (int i=0; i<binList.Count; i++)
						{
							binArray[i+1] = binList[i];
						}

						categoryFilter = EditorGUILayout.Popup ("Category filter:", categoryFilter, binArray);
					}
					else
					{
						categoryFilter = 0;
					}
					EditorGUILayout.Space ();
				}

				List<Recipe> recipesInFilter = new List<Recipe> ();
				for (int i = 0; i < recipes.Count; i++)
				{
					if (!string.IsNullOrEmpty (nameFilter) && !recipes[i].label.ToLower ().Contains (nameFilter.ToLower ()))
					{
						if (selectedRecipe == recipes[i])
						{
							DeactivateAllRecipes ();
						}
						continue;
					}
					if (categoryFilter > 0 && bins.Count > (categoryFilter-1))
					{
						var item = GetItem (recipes[i].resultID);
						if (item != null && item.binID != bins[categoryFilter-1].id)
						{
							if (selectedRecipe == recipes[i])
							{
								DeactivateAllRecipes ();
							}
							continue;
						}
					}
					recipesInFilter.Add (recipes[i]);
				}

				CustomGUILayout.BeginScrollView (ref scrollPos, recipesInFilter.Count);
				for (int i = 0; i < recipesInFilter.Count; i++)
				{
					Recipe recipe = recipesInFilter[i];
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = recipe.label;
					if (string.IsNullOrEmpty (buttonLabel))
					{
						buttonLabel = "(Untitled)";	
					}
					
					if (GUILayout.Toggle (selectedRecipe == recipe, recipe.id + ": " + buttonLabel, "Button"))
					{
						if (selectedRecipe != recipe)
						{
							DeactivateAllRecipes ();
							ActivateRecipe (recipe);
						}
					}

					Rect buttonRect = GUILayoutUtility.GetLastRect ();
					if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
					{
						lastDragRecipeOver = recipe;
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (recipe);
					}
					
					EditorGUILayout.EndHorizontal ();

					if (IsDraggingRecipe ())
					{
						CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
					}
				}
				CustomGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Create new recipe"))
				{
					Undo.RecordObject (this, "Create inventory recipe");
					
					Recipe newRecipe = new Recipe (GetIDArrayRecipe ());
					recipes.Add (newRecipe);
					DeactivateAllRecipes ();
					ActivateRecipe (newRecipe);
				}
				CustomGUILayout.EndVertical ();
			}

			if (selectedRecipe != null && recipes.Contains (selectedRecipe))
			{
				string apiPrefix = "AC.KickStarter.inventoryManager.GetRecipe (" + selectedRecipe.id + ")";

				EditorGUILayout.Space ();
				showCraftingProperties = CustomGUILayout.ToggleHeader (showCraftingProperties, "Recipe '" + selectedRecipe.label + "' properties");
				if (showCraftingProperties)
				{
					CustomGUILayout.BeginVertical ();
					selectedRecipe.label = CustomGUILayout.TextField ("Name:", selectedRecipe.label, apiPrefix + ".label", "The recipe's editor name");
					
					int i = GetArraySlot (selectedRecipe.resultID);
					i = CustomGUILayout.Popup ("Resulting item:", i, GetLabelList ());
					selectedRecipe.resultID = items[i].id;
					
					selectedRecipe.useSpecificSlots = CustomGUILayout.Toggle ("Uses specific pattern?", selectedRecipe.useSpecificSlots, apiPrefix + ".useSpecificSlots", "If True, then the ingredients must be placed in specific slots within a Crafting menu element");
					selectedRecipe.actionListOnCreate = ActionListAssetMenu.AssetGUI ("ActionList when create:", selectedRecipe.actionListOnCreate, "Recipe_" + selectedRecipe.label + "_OnCreate", apiPrefix + ".actionListOnCreate", "The ActionList asset to run when the recipe is created", null, showALAEditor);

					selectedRecipe.onCreateRecipe = (OnCreateRecipe) CustomGUILayout.EnumPopup ("When click on result:", selectedRecipe.onCreateRecipe, apiPrefix + ".onCreateRecipe", "What happens when the recipe is created");
					if (selectedRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
					{
						selectedRecipe.invActionList = ActionListAssetMenu.AssetGUI ("ActionList when click:", selectedRecipe.invActionList, "Recipe_" + selectedRecipe.label + "_OnClick", apiPrefix + ".invActionList", "The ActionListAsset to run when clicking on the resulting item", null, showALAEditor);
					}
					CustomGUILayout.EndVertical ();
				}

				EditorGUILayout.Space ();

				if (selectedRecipe != null)
				{
					showCraftingIntegredients = CustomGUILayout.ToggleHeader (showCraftingIntegredients, "Recipe '" + selectedRecipe.label + "' ingredients");
					if (showCraftingIntegredients)
					{
						CustomGUILayout.BeginVertical ();
						foreach (Ingredient ingredient in selectedRecipe.ingredients)
						{
							EditorGUILayout.BeginHorizontal ();

							if (GUILayout.Toggle (selectedIngredient == ingredient, selectedRecipe.ingredients.IndexOf (ingredient) + ": " + ingredient.EditorLabel, "Button"))
							{
								if (selectedIngredient != ingredient)
								{
									DeactivateAllIngredients ();
									ActivateIngredient (ingredient);
								}
							}

							if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
							{
								SideMenu (ingredient);
							}

							EditorGUILayout.EndHorizontal ();
						}
					
						EditorGUILayout.Space ();
						if (GUILayout.Button("Add new ingredient"))
						{
							Undo.RecordObject (this, "Add recipe ingredient");
						
							Ingredient newIngredient = new Ingredient ();
							selectedRecipe.ingredients.Add (newIngredient);
							selectedIngredient = newIngredient;
						}

						CustomGUILayout.EndVertical ();
					}
				}

				if (selectedRecipe != null && selectedIngredient != null)
				{
					EditorGUILayout.Space ();

					showCraftingIntegredientProperties = CustomGUILayout.ToggleHeader (showCraftingIntegredientProperties, "Recipe '" + selectedRecipe.label + "' ingredient " + selectedRecipe.ingredients.IndexOf (selectedIngredient));
					if (showCraftingIntegredientProperties)
					{
						selectedIngredient.ShowGUI (GetArraySlot (selectedIngredient.ItemID), GetLabelList (), apiPrefix, selectedRecipe);
					}
				}
			}
		}


		private List<int> GetIDList ()
		{
			List<int> idList = new List<int>();
			foreach (InvItem item in items)
			{
				idList.Add (item.id);
			}
			
			idList.Sort ();

			return idList;
		}
		
		
		private int[] GetIDArrayProperty ()
		{
			List<int> idArray = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				idArray.Add (invVar.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int[] GetIDArrayRecipe ()
		{
			List<int> idArray = new List<int>();
			foreach (Recipe recipe in recipes)
			{
				idArray.Add (recipe.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		public int GetArraySlot (int _id)
		{
			int i = 0;
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		public string[] GetLabelList ()
		{
			List<string> labelList = new List<string>();
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			return labelList.ToArray ();
		}
		
		
		public int GetBinSlot (int _id)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private void SetTab (int tab)
		{
			showItemsTab = (tab == 0);
			showBinsTab = (tab == 1);
			showCraftingTab = (tab == 2);
			showPropertiesTab = (tab == 3);
			showDocumentsTab = (tab == 4);
			showObjectivesTab = (tab == 5);
		}


		private void FindReferences (InvItem item)
		{
			if (item == null) return;

			if (EditorUtility.DisplayDialog ("Search '" + item.label + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the inventory item.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search other items
					foreach (InvItem otherItem in items)
					{
						if (otherItem != item)
						{
							int thisNumReferences = otherItem.GetNumItemReferences (item.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Inventory item '" + item.label + "' in Inventory item " + otherItem.EditorLabel);
							}
						}
					}

					// Search crafting
					foreach (Recipe recipe in recipes)
					{
						int thisNumReferences = recipe.GetNumItemReferences (item.id);
						if (thisNumReferences > 0)
						{
							totalNumReferences += thisNumReferences;
							ACDebug.Log ("Found " + thisNumReferences  + " reference(s) to Inventory item '" + item.label + "' in Recipe " + recipe.EditorLabel);
						}
					}

					// Search Players
					if (KickStarter.settingsManager)
					{
						Player[] players = KickStarter.settingsManager.GetAllPlayerPrefabs ();
						foreach (Player player in players)
						{
							MonoBehaviour[] playerComponents = player.gameObject.GetComponentsInChildren<MonoBehaviour> ();
							for (int i = 0; i < playerComponents.Length; i++)
							{
								MonoBehaviour currentObj = playerComponents[i];
								IItemReferencer currentComponent = currentObj as IItemReferencer;
								if (currentComponent != null)
								{
									ActionList.logSuffix = string.Empty;
									int thisNumReferences = currentComponent.GetNumItemReferences (item.id);
									if (thisNumReferences > 0)
									{
										totalNumReferences += thisNumReferences;
										ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Inventory item '" + item.label + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' on Player '" + player.gameObject.name + "' " + ActionList.logSuffix, player);
									}
								}
							}
						}
					}

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();
					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IItemReferencer currentComponent = currentObj as IItemReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.GetNumItemReferences (item.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Inventory item '" + item.label + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "' " + ActionList.logSuffix, currentObj);
								}
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.GetNumItemReferences (item.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Inventory item '" + item.label + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
							}
						}
					}

					EditorUtility.DisplayDialog ("Inventory search complete", "In total, found " + totalNumReferences + " reference(s) to inventory item '" + item.label + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		public void ChangeItemID (int oldID, int newID)
		{
			InvItem item = GetItem (oldID);
			if (item == null || oldID == newID) return;

			if (GetItem (newID) != null)
			{
				ACDebug.LogWarning ("Cannot update Inventory item " + item.EditorLabel + " to ID " + newID + " because another Inventory item uses the same ID");
				return;
			}

			if (EditorUtility.DisplayDialog ("Update '" + item.EditorLabel + "' references?", "The Editor will update assets, and active scenes listed in the Build Settings, that reference the inventory item.  It is recommended to back up the project first. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search other items
					foreach (InvItem otherItem in items)
					{
						if (otherItem != item)
						{
							int thisNumReferences = otherItem.UpdateItemReferences (oldID, newID);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Inventory item '" + item.EditorLabel + "' in Inventory item " + otherItem.EditorLabel);
							}
						}
					}

					// Search crafting
					foreach (Recipe recipe in recipes)
					{
						int thisNumReferences = recipe.UpdateItemReferences (oldID, newID);
						if (thisNumReferences > 0)
						{
							totalNumReferences += thisNumReferences;
							ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Inventory item '" + item.EditorLabel + "' in Recipe " + recipe.EditorLabel);
						}
					}

					// Search Players
					if (KickStarter.settingsManager)
					{
						Player[] players = KickStarter.settingsManager.GetAllPlayerPrefabs ();
						foreach (Player player in players)
						{
							MonoBehaviour[] playerComponents = player.gameObject.GetComponentsInChildren<MonoBehaviour> ();
							for (int i = 0; i < playerComponents.Length; i++)
							{
								MonoBehaviour currentObj = playerComponents[i];
								IItemReferencer currentComponent = currentObj as IItemReferencer;
								if (currentComponent != null)
								{
									ActionList.logSuffix = string.Empty;
									int thisNumReferences = currentComponent.UpdateItemReferences (oldID, newID);
									if (thisNumReferences > 0)
									{
										totalNumReferences += thisNumReferences;
										EditorUtility.SetDirty (player);
										ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Inventory item '" + item.EditorLabel + "' in '" + currentComponent.GetType () + "' " + currentObj.name + " on Player '" + player.gameObject.name + "'" + ActionList.logSuffix, player);
									}
								}
							}
						}
					}

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();
					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						bool modifiedScene = false;
						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IItemReferencer currentComponent = currentObj as IItemReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.UpdateItemReferences (oldID, newID);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									modifiedScene = true;
									EditorUtility.SetDirty (currentObj);
									ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Inventory item '" + item.EditorLabel + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "'" + ActionList.logSuffix, currentObj);
								}
							}
						}

						if (modifiedScene)
						{
							UnityVersionHandler.SaveScene ();
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.UpdateItemReferences (oldID, newID);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								EditorUtility.SetDirty (actionListAsset);
								ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Inventory item '" + item.EditorLabel + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
							}
						}
					}

					item.id = newID;
					EditorUtility.SetDirty (this);

					EditorUtility.DisplayDialog ("Update complete", "In total, updated " + totalNumReferences + " reference(s) to inventory item '" + item.EditorLabel + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		private void FindReferences (Document document)
		{
			if (document == null) return;

			if (EditorUtility.DisplayDialog ("Search '" + document.Title + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the document.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IDocumentReferencer currentComponent = currentObj as IDocumentReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.GetNumDocumentReferences (document.ID);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Document '" + document.Title + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "'" + ActionList.logSuffix, currentObj);
								}
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.GetNumDocumentReferences (document.ID);
							if (thisNumReferences > 0)
							{
								ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Document '" + document.Title + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
								totalNumReferences += thisNumReferences;
							}
						}
					}

					EditorUtility.DisplayDialog ("Document search complete", "In total, found " + totalNumReferences + " references to document '" + document.Title + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		public void ChangeDocumentID (int oldID, int newID)
		{
			Document document = GetDocument (oldID);
			if (document == null || oldID == newID) return;

			if (GetDocument (newID) != null)
			{
				ACDebug.LogWarning ("Cannot update Document " + document.Title + " to ID " + newID + " because another Document uses the same ID");
				return;
			}

			if (EditorUtility.DisplayDialog ("Update '" + document.Title + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the document.  The current scene will need to be saved and listed to be included in the search process. It is recommended to back up the project first. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						bool modifiedScene = false;

						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IDocumentReferencer currentComponent = currentObj as IDocumentReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.UpdateDocumentReferences (oldID, newID);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									EditorUtility.SetDirty (currentObj);
									modifiedScene = true;
									ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Document '" + document.Title + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "'" + ActionList.logSuffix, currentObj);
								}
							}
						}

						if (modifiedScene)
						{
							UnityVersionHandler.SaveScene ();
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.UpdateDocumentReferences (oldID, newID);
							if (thisNumReferences > 0)
							{
								ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Document '" + document.Title + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
								totalNumReferences += thisNumReferences;
							}
						}
					}

					document.ID = newID;
					EditorUtility.SetDirty (this);
					EditorUtility.DisplayDialog ("Document update complete", "In total, updated " + totalNumReferences + " references to document '" + document.Title + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		private void FindReferences (Objective objective)
		{
			if (objective == null) return;

			if (EditorUtility.DisplayDialog ("Search '" + objective.Title + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the objective.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IObjectiveReferencer currentComponent = currentObj as IObjectiveReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.GetNumObjectiveReferences (objective.ID);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Objective '" + objective.Title + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "'" + ActionList.logSuffix, currentObj);
								}
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.GetNumObjectiveReferences (objective.ID);
							if (thisNumReferences > 0)
							{
								ACDebug.Log ("Found " + thisNumReferences + " reference(s) to Objective '" + objective.Title + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
								totalNumReferences += thisNumReferences;
							}
						}
					}

					EditorUtility.DisplayDialog ("Document search complete", "In total, found " + totalNumReferences + " reference(s) to Objective '" + objective.Title + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		public void ChangeObjectiveID (int oldID, int newID)
		{
			Objective objective = GetObjective (oldID);
			if (objective == null) return;

			if (GetObjective (newID) != null)
			{
				ACDebug.LogWarning ("Cannot update Objective " + objective.Title + " to ID " + newID + " because another Objective uses the same ID");
				return;
			}

			if (EditorUtility.DisplayDialog ("Update '" + objective.Title + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the objective.  The current scene will need to be saved and listed to be included in the search process.  It is recommended to back up the project first.  Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					int totalNumReferences = 0;

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						bool modifiedScene = false;

						MonoBehaviour[] sceneObjects = UnityVersionHandler.FindObjectsOfType<MonoBehaviour> ();
						for (int i = 0; i < sceneObjects.Length; i++)
						{
							MonoBehaviour currentObj = sceneObjects[i];
							IObjectiveReferencer currentComponent = currentObj as IObjectiveReferencer;
							if (currentComponent != null)
							{
								ActionList.logSuffix = string.Empty;
								int thisNumReferences = currentComponent.UpdateObjectiveReferences (oldID, newID);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									EditorUtility.SetDirty (currentObj);
									modifiedScene = true;
									ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Objective '" + objective.Title + "' in " + currentComponent.GetType () + " '" + currentObj.name + "' in scene '" + sceneFile + "'" + ActionList.logSuffix, currentObj);
								}
							}
						}

						if (modifiedScene)
						{
							UnityVersionHandler.SaveScene ();
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (KickStarter.speechManager)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							ActionList.logSuffix = string.Empty;
							int thisNumReferences = actionListAsset.UpdateObjectiveReferences (oldID, newID);
							if (thisNumReferences > 0)
							{
								ACDebug.Log ("Updated " + thisNumReferences + " reference(s) to Objective '" + objective.Title + "' in ActionList asset " + actionListAsset.name + ActionList.logSuffix, actionListAsset);
								EditorUtility.SetDirty (actionListAsset);
								totalNumReferences += thisNumReferences;
							}
						}
					}

					objective.ID = newID;
					EditorUtility.SetDirty (this);
					EditorUtility.DisplayDialog ("Document update complete", "In total, updated " + totalNumReferences + " reference(s) to Objective '" + objective.Title + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}

		#endif


		/**
		 * <summary>Gets an inventory property.</summary>
		 * <param name = "ID">The ID number of the property to get</param>
		 * <returns>The inventory property.</returns>
		 */
		public InvVar GetProperty (int ID)
		{
			if (invVars.Count > 0 && ID >= 0)
			{
				foreach (InvVar var in invVars)
				{
					if (var.id == ID)
					{
						return var;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Gets an inventory property.</summary>
		 * <param name = "label">The name of the property to get</param>
		 * <returns>The inventory property.</returns>
		 */
		public InvVar GetProperty (string label)
		{
			if (invVars.Count > 0 && !string.IsNullOrEmpty (label))
			{
				foreach (InvVar var in invVars)
				{
					if (var.label == label)
					{
						return var;
					}
				}
			}
			return null;
		}
		

		/**
		 * <summary>Gets a Document.</summary>
		 * <param name = "ID">The ID number of the Document to find</param>
		 * <returns>The Document</returns>
		*/
		public Document GetDocument (int ID)
		{
			foreach (Document document in documents)
			{
				if (document.ID == ID)
				{
					return document;
				}
			}

			return null;
		}


		/**
		 * <summary>Gets an Objective.</summary>
		 * <param name = "ID">The ID number of the Objective to find</param>
		 * <returns>The Objective</returns>
		*/
		public Objective GetObjective (int ID)
		{
			foreach (Objective objective in objectives)
			{
				if (objective.ID == ID)
				{
					return objective;
				}
			}

			return null;
		}

		
		/**
		 * <summary>Gets an inventory item's label.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item's label</returns>
		 */
		public string GetLabel (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item.label;
				}
			}
			return string.Empty;
		}
		
		
		/**
		 * <summary>Gets an inventory item.</summary>
		 * <param name = "_name">The name of the InvItem to find</param>
		 * <returns>The inventory item</returns>
		 */
		public InvItem GetItem (string _name)
		{
			foreach (InvItem item in items)
			{
				if (item.label == _name)
				{
					return item;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets an inventory item.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item</returns>
		 */
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a Recipe.</summary>
		 * <param name = "_id">The ID number of the Recipe to find</param>
		 * <returns>The Recipe</returns>
		 */
		public Recipe GetRecipe (int _id)
		{
			foreach (Recipe recipe in recipes)
			{
				if (recipe.id == _id)
				{
					return recipe;
				}
			}
			return null;
		}


		/** Gets the ID of the first-defined category avaiable for Inventory items, or -1 otherwise */
		public int GetFirstItemsCategoryID ()
		{
			if (bins == null) return -1;
			foreach (InvBin bin in bins)
			{
				if (bin.forItems)
				{
					return bin.id;
				}
			}
			return -1;
		}


		public bool IsInItemsCategory (int binID)
		{
			InvBin bin = GetCategory (binID);
			if (bin == null) return false;
			return bin.forItems;
		}


		public bool IsInDocumentsCategory (int binID)
		{
			InvBin bin = GetCategory (binID);
			if (bin == null) return false;
			return bin.forDocuments;
		}


		public bool IsInObjectivesCategory (int binID)
		{
			InvBin bin = GetCategory (binID);
			if (bin == null) return false;
			return bin.forObjectives;
		}

		/** Gets the ID of the first-defined category avaiable for Documents, or -1 otherwise */
		public int GetFirstDocumentsCategoryID ()
		{
			if (bins == null) return -1;
			foreach (InvBin bin in bins)
			{
				if (bin.forDocuments)
				{
					return bin.id;
				}
			}
			return -1;
		}


		/**
		 * <summary>Gets an array of Objectives that belong in a given category</summary>
		 * <param name = "categoryID">The ID of the category</param>
		 * <returns>An array of all Objectives in the given category</returns>
		 */
		public Objective[] GetObjectivesInCategory (int categoryID)
		{
			List<Objective> _objectives = new List<Objective> ();

			foreach (Objective objective in objectives)
			{
				if (objective.binID == categoryID)
				{
					_objectives.Add (objective);
				}
			}

			return _objectives.ToArray ();
		}


		/** Gets the ID Of the first-defined category avaiable for Objectives, or -1 otherwise */
		public int GetFirstObjectivessCategoryID ()
		{
			if (bins == null) return -1;
			foreach (InvBin bin in bins)
			{
				if (bin.forObjectives)
				{
					return bin.id;
				}
			}
			return -1;
		}


		/**
		 * <summary>Gets an inventory category.</summary>
		 * <param name = "categoryID">The ID number of the inventory category to find</param>
		 * <returns>The inventory category</returns>
		 */
		public InvBin GetCategory (int categoryID)
		{
			foreach (InvBin bin in bins)
			{
				if (bin.id == categoryID)
				{
					return bin;
				}
			}
			return null;
		}
		

		/**
		 * <summary>Gets an inventory category.</summary>
		 * <param name = "label">The name of the inventory category to find</param>
		 * <returns>The inventory category</returns>
		 */
		public InvBin GetCategory (string label)
		{
			foreach (InvBin bin in bins)
			{
				if (bin.label == label)
				{
					return bin;
				}
			}
			return null;
		}
		

		/**
		 * <summary>Gets an array of all inventory items in a given category</summary>
		 * <param name = "categoryID">The ID number of the category in question</param>
		 * <returns>An array of all inventory items in the category</returns>
		 */
		public InvItem[] GetItemsInCategory (int categoryID)
		{
			List<InvItem> itemsList = new List<InvItem>();
			foreach (InvItem item in items)
			{
				if (item.binID == categoryID)
				{
					itemsList.Add (item);
				}
			}
			return itemsList.ToArray ();
		}


		/**
		 * <summary>Gets an array of all Documents in a given category</summary>
		 * <param name = "categoryID">The ID number of the category in question</param>
		 * <returns>An array of all Documents in the category</returns>
		 */
		public Document[] GetDocumentsInCategory (int categoryID)
		{
			List<Document> documentsList = new List<Document> ();
			foreach (Document document in documents)
			{
				if (document.binID == categoryID)
				{
					documentsList.Add (document);
				}
			}
			return documentsList.ToArray ();
		}


		/**
		 * <summary>Checks if a given Objective is per-Player, i.e. each player has their own instance of the Objective</summary>
		 * <param name = "objectiveID">The ID of the Objective</param>
		 * <returns>True if the Objective is per-Player</returns>
		 */
		public bool ObjectiveIsPerPlayer (int objectiveID)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				foreach (Objective objective in objectives)
				{
					if (objective.ID == objectiveID)
					{
						return objective.perPlayer;
					}
				}
				ACDebug.LogWarning ("An Objective with ID=" + objectiveID + " could not be found.");
				return false;
			}
			return false;
		}


		#if UNITY_EDITOR
		
		public int ChooseCategoryGUI (string label, int binID, bool forItems, bool forDocuments, bool forObjectives, string apiPrefix = "", string tooltip = "", bool addNoneOption = false)
		{
			if (bins == null || bins.Count == 0)
			{
				return -1;
			}

			// Don't show list if no parameters of the correct type are present
			bool found = false;
			foreach (InvBin bin in bins)
			{
				if ((forItems && bin.forItems) ||
					(forDocuments && bin.forDocuments) ||
					(forObjectives && bin.forObjectives))
				{
					found = true;
				}
			}
			if (!found)
			{
				return -1;
			}

			int chosenNumber = 0;
			List<PopupSelectData> popupSelectDataList = new List<PopupSelectData> ();

			if (addNoneOption)
			{
				popupSelectDataList.Add (new PopupSelectData (-1, "None", -1));
			}

			for (int i = 0; i < bins.Count; i++)
			{
				if ((forItems && !bins[i].forItems) ||
					(forDocuments && !bins[i].forDocuments) ||
					(forObjectives && !bins[i].forObjectives))
				{
					continue;
				}

				PopupSelectData popupSelectData = new PopupSelectData (bins[i].id, bins[i].label, i);
				popupSelectDataList.Add (popupSelectData);

				if (popupSelectData.ID == binID)
				{
					chosenNumber = popupSelectDataList.Count - 1;
				}
			}

			List<string> labelList = new List<string> ();

			foreach (PopupSelectData popupSelectData in popupSelectDataList)
			{
				labelList.Add (popupSelectData.label);
			}

			chosenNumber = CustomGUILayout.Popup (label, chosenNumber, labelList.ToArray (),  apiPrefix, tooltip);

			if (chosenNumber < 0)
			{
				return -1;
			}

			if (chosenNumber == 0 && addNoneOption)
			{
				return -1;
			}

			int rootIndex = popupSelectDataList[chosenNumber].rootIndex;
			return bins[rootIndex].id;
		}


		private void OnCompleteDragItem (object data)
		{
			InvItem item = (InvItem) data;
			if (item == null) return;

			int dragIndex = items.IndexOf (item);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				InvItem tempItem = item;

				items.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					items.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					items.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllItems ();
			ActivateItem (item);
		}


		private bool IsDraggingItem ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragItemKey);
			if (dragObject != null && dragObject is InvItem)
			{
				return true;
			}
			return false;
		}


		private void OnCompleteDragBin (object data)
		{
			InvBin bin = (InvBin) data;
			if (bin == null) return;

			int dragIndex = bins.IndexOf (bin);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				InvBin tempItem = bin;

				bins.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					bins.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					bins.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllCategories ();
			ActivateCategory (bin);
		}


		private bool IsDraggingBin ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragBinKey);
			if (dragObject != null && dragObject is InvBin)
			{
				return true;
			}
			return false;
		}


		private void OnCompleteDragVar (object data)
		{
			InvVar var = (InvVar) data;
			if (var == null) return;

			int dragIndex = invVars.IndexOf (var);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				InvVar tempItem = var;

				invVars.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					invVars.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					invVars.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllInvVars ();
			ActivateItem (var);
		}


		private bool IsDraggingVar ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragVarKey);
			if (dragObject != null && dragObject is InvVar)
			{
				return true;
			}
			return false;
		}


		private void OnCompleteDragRecipe (object data)
		{
			Recipe recipe = (Recipe) data;
			if (recipe == null) return;

			int dragIndex = recipes.IndexOf (recipe);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				Recipe tempItem = recipe;

				recipes.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					recipes.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					recipes.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllRecipes ();
			ActivateRecipe (recipe);
		}


		private bool IsDraggingRecipe ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragRecipeKey);
			if (dragObject != null && dragObject is Recipe)
			{
				return true;
			}
			return false;
		}


		private void OnCompleteDragObjective (object data)
		{
			Objective objective = (Objective) data;
			if (objective == null) return;

			int dragIndex = objectives.IndexOf (objective);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				Objective tempItem = objective;

				objectives.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					objectives.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					objectives.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllObjectives ();
			ActivateObjective (objective);
		}


		private bool IsDraggingObjective ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragObjectiveKey);
			if (dragObject != null && dragObject is Objective)
			{
				return true;
			}
			return false;
		}


		private void OnCompleteDragDocument (object data)
		{
			Document document = (Document) data;
			if (document == null) return;

			int dragIndex = documents.IndexOf (document);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				Document tempItem = document;

				documents.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					documents.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					documents.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (this);
			}

			DeactivateAllDocuments ();
			ActivateDocument (document);
		}


		private bool IsDraggingDocument ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragDocumentKey);
			if (dragObject != null && dragObject is Document)
			{
				return true;
			}
			return false;
		}
#endif

	}

}