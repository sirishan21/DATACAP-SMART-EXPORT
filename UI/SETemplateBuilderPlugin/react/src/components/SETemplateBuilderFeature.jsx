import React from "react";
import {
  Form,
  FormLabel,
  TextInput,
  Button,
  Dropdown,
  Tabs,
  Tab,
  FormGroup,
  RadioButtonGroup,
  RadioButton,
  fieldset,
  legend,
  Checkbox,
  ModalWrapper,
  TableContainer,
  Table,
  TableHead,
  TableRow,
  TableBody,
  TableHeader,
  TableCell,
  DataTable,
  CodeSnippet,
  Select,
  SelectItem,
  TableToolbar,
  TableToolbarContent,
  TableSelectAll,
  TableSelectRow,
  TableBatchActions,
  TableBatchAction,
  TableToolbarMenu,
  TableToolbarAction,
  DynamicRows,
  StructuredListWrapper,
  StructuredListHead,
  StructuredListRow,
  StructuredListCell,
  StructuredListBody,
  OverflowMenuItem,
  OverflowMenu,
  ProgressIndicator,
  ProgressStep,
  Modal,
  OrderedList,
  ListItem,
} from "carbon-components-react";
import "carbon-components/css/carbon-components.css";
import "../styles/sETemplateBuilderFeature.scss";
const {
  TrashCan32,
  Edit32,
  InformationFilled32,
} = require("@carbon/icons-react");

class SampleReactFeature extends React.Component {
  constructor(props) {
    super(props);
  }
  state = {
    isDevelopent: false,
    selectedTab: 0, //bmk
    placeholderId: 100,
    disableTab1: false,
    disableTab2: true,
    disableTab3: true,
    disableTab4: true,
    disableTab5: true,
    disableTab6: true,
    invalidTab1: false,
    invalidTab2: false,
    invalidTab3: false,
    invalidTab4: false,
    invalidTab5: false,
    invalidTab6: false,
    selectedApplication: "True",
    selectedLevel: "",
    selectedAppendToFile: "",
    selectedLocale: "",
    selectedFileName: "",
    selectedFileNameSmartP: "",
    selectedExportFormat: "",
    selectedOutputFolder: "",
    selectedOutputFolderSmartP: "",
    selectedDelimiter: "",
    selectedDocumentType: "",
    selectedDocumentsForBatchLevel: [],
    selectedPage: "",
    selectedPagesForDocLevel: [],
    selectedFields: [],
    selectedModalGroup: "",
    changedFieldsLabelsMap: new Map(),
    selectedFieldsForDocLevelMap: new Map(),
    selectedDocPageMap: new Map(),
    currentSelectedPageForFields: "",
    currentSelectedDocInFieldTab: "",
    currentSelectedDocForPages: "",
    docListInFieldtab: [],
    selectedFieldIdsArray: [],
    selectedPagesArray: [],
    selectedModalField: "",
    selectedModalOperator: "",
    selectedModalValue: "",
    selectedModalAndOrBlank: "",
    conditionsJson: [],
    editConditionIndex: -1,
    dcoTree: {},
    ddDatacapApp: [],
    ddLevel: ["Field", "Page", "Document", "Batch"],
    ddSmartParam: ["ID", "B.TYPE", "B.ID", "D.TYPE", "P.TYPE"],
    ddTrueFalse: ["True", "False"],
    ddYesNo: ["Yes", "No"],
    ddLocale: ["en-US"],
    ddExportFormat: ["CSV", "KeyValue"],
    delimiterJson: {
      CSV: ["Comma", "Tab"],
      KeyValue: ["  :  ", "  =  "],
    },
    ddDelimiter: [],
    pageFieldInfoJson: [],
    docFieldInfoJson: [],
    rbDocumentTypes: [],
    rbPages: [],
    cbFields: [],
    rbFields: [],
    ddOperators: ["Greater Than", "Equals", "Lesser Than"],
    ddAndOrBlank: ["AND", "OR", "Blank"],
    ddGroups: ["Group 1"],
    groupIndex: 2,
    selectedTableCbSelect: [],
    headerData: [
      //   {
      //     header: "Group",
      //     key: "group",
      //   },
      {
        header: "Field(s)",
        key: "field",
      },
      {
        header: "Condition(s)",
        key: "condition",
      },
    ],

    headerData2: [
      {
        header: "Group",
        key: "group",
      },
      {
        header: "Field(s)",
        key: "field",
      },
      {
        header: "Operator",
        key: "operator",
      },
      {
        header: "Value",
        key: "value",
      },
      {
        header: "Join Operator",
        key: "joinOperator",
      },
      //   {
      //     header: "Add Condition",
      //     key: "add",
      //   },
    ],

    headerData3: [
      {
        header: "Fields to be Exported",
        key: "fields",
      },
      {
        header: "Condition(s)",
        key: "conditions",
      },
    ],

    rowData2: [],
    rowData3: [],

    rowData: [
      //   {
      //     id: "1",
      //     // group: "Group1",
      //     field: "Car Type, Pickup Date, Return Date, Pickup Location",
      //     condition: "",
      //   },
    ],

    rowId: 200,
    idRowData3: 400,
    arrayConditions: [],

    codeSnippet: "",

    nextButtonDisabled: true,
    isInfoModalOpen: false,
  };

  uiPreviousButton = () => {
    if (this.state.selectedTab > 0 && this.state.selectedTab < 6) {
      return (
        <Button small className="btn-prv" onClick={this.clickedPrevious}>
          &#60;&#60; Previous
        </Button>
      );
    }
  };

  setDataForDevelopement = () => {
    this.setState(
      {
        selectedLevel: "Page",
        ddDatacapApp: ["Travel Docs", "Docs of Travel"],
        rbDocumentTypes: [
          { id: "1", name: "Car Rental" },
          { id: "2", name: "Flight" },
          { id: "3", name: "Hotel" },
          { id: "4", name: "Test" },
        ],
        rbPages: [
          { id: "1", name: "Rental Agreement" },
          { id: "2", name: "Optional Insurance" },
          { id: "3", name: "Optional Insurance de" },
        ],
        cbFields: [
          { id: "1", name: "Car Type" },
          { id: "2", name: "Pickup Date" },
          { id: "3", name: "Return Date" },
          { id: "4", name: "Pickup Location" },
        ],
        cbFields2: [
          { id: "1", name: "Car Type" },
          { id: "2", name: "Pickup Date" },
          { id: "3", name: "Return Date" },
          { id: "4", name: "Pickup Location" },
        ],
      },
      () => {
        this.validate();
      }
    );
  };

  componentDidMount() {
    if (this.state.isDevelopent) {
      this.setDataForDevelopement();
      return;
    }
    ecm.model.Request.invokePluginService(
      "SETemplateBuilderPlugin",
      "SETemplateBuilderPluginService",
      {
        requestParams: { methodName: "getApplications" },
        requestCompleteCallback: (response) => {
          this.setState({
            ddDatacapApp: response.Applications,
          });
          console.log("response", response);
        },
        requestFailedCallback: (error) => {
          console.log("request failed with error : ", error);
        },
        requestHeaders: { "Cache-Control": "no-cache" },
      }
    );
  }
  uiNextButton = () => {
    if (this.state.selectedTab >= 0 && this.state.selectedTab < 5) {
      return (
        <Button
          small
          className="btn-next"
          disabled={this.state.nextButtonDisabled}
          onClick={this.clickedNext}
        >
          Next &#62;&#62;
        </Button>
      );
    }
  };

  clickedNext = (event) => {
    event.preventDefault();
    if (this.state.selectedTab >= 0 && this.state.selectedTab < 5) {
      this.setState(
        {
          selectedTab: this.state.selectedTab + 1,
        },
        () => {
          this.validate();
        }
      );
    }

    if (
      this.state.selectedTab === 2 &&
      this.state.selectedDocumentsForBatchLevel.length > 0
    ) {
      this.setState({
        currentSelectedDocForPages: this.state
          .selectedDocumentsForBatchLevel[0],
        rbPages: this.state.dcoTree.documentPageMap[
          this.state.selectedDocumentsForBatchLevel[0]
        ],
      });
    }

    if (this.state.selectedTab === 3) {
      if (
        this.state.selectedLevel === "Document" &&
        this.state.selectedPagesForDocLevel.length > 0
      ) {
        this.setState({
          currentSelectedPageForFields: this.state.selectedPagesForDocLevel[0],
          cbFields: this.state.dcoTree.pageFieldMap[
            this.state.selectedPagesForDocLevel[0]
          ],
        });
      } else if (
        this.state.selectedLevel === "Batch" &&
        this.state.selectedDocPageMap.size > 0
      ) {
        console.log(
          "selectedDocumentsForBatchLevel ",
          this.state.selectedDocumentsForBatchLevel
        );
        this.setState({
          currentSelectedDocInFieldTab: this.state
            .selectedDocumentsForBatchLevel[0],
          selectedPagesForDocLevel: this.state.selectedDocPageMap.get(
            this.state.selectedDocumentsForBatchLevel[0]
          ),
          currentSelectedPageForFields: this.state.selectedDocPageMap.get(
            this.state.selectedDocumentsForBatchLevel[0]
          )[0],
          cbFields: this.state.dcoTree.pageFieldMap[
            this.state.selectedDocPageMap.get(
              this.state.selectedDocumentsForBatchLevel[0]
            )[0]
          ],
        });
      }
    }

    if (this.state.selectedTab == 4) {
      //¯this.updateSelectedFieldValues();
      this.getSETemplate();
    }
  };

  clickedPrevious = () => {
    if (this.state.selectedTab > 0 && this.state.selectedTab <= 5) {
      this.setState(
        {
          selectedTab: this.state.selectedTab - 1,
        },
        () => {
          this.validate();
        }
      );
    }
  };

  validate = () => {
    if (this.state.isDevelopent) {
      this.setState({
        nextButtonDisabled: false,
        disableTab2: false,
        disableTab3: false,
        disableTab4: false,
        disableTab5: false,
        disableTab6: false,
      });

      return;
    }

    if (this.state.selectedTab === 0) {
      if (
        this.state.selectedApplication !== "" &&
        this.state.selectedLevel !== ""
      ) {
        this.setState({
          nextButtonDisabled: false,
          disableTab2: false,
        });
      } else {
        this.setState({
          nextButtonDisabled: true,
          disableTab2: true,
          disableTab3: true,
          disableTab4: true,
          disableTab5: true,
          disableTab6: true,
        });
      }
    } else if (this.state.selectedTab === 1) {
      if (
        this.state.selectedAppendToFile !== "" &&
        this.state.selectedLocale !== "" &&
        (this.state.selectedFileName !== "" ||
          this.state.selectedFileNameSmartP !== "") &&
        this.state.selectedExportFormat !== "" &&
        (this.state.selectedOutputFolder !== "" ||
          this.state.selectedOutputFolderSmartP !== "") &&
        this.state.selectedDelimiter !== ""
      ) {
        this.setState({
          nextButtonDisabled: false,
          disableTab3: false,
        });
      } else {
        this.setState({
          nextButtonDisabled: true,
          disableTab3: true,
          disableTab4: true,
          disableTab5: true,
          disableTab6: true,
        });
      }
    } else if (this.state.selectedTab === 2) {
      if (
        this.state.selectedDocumentType !== "" ||
        this.state.selectedDocumentsForBatchLevel.length > 0
      ) {
        this.setState({
          nextButtonDisabled: false,
          disableTab4: false,
        });
      } else {
        this.setState({
          nextButtonDisabled: true,
          disableTab4: true,
          disableTab5: true,
          disableTab6: true,
        });
      }
    } else if (this.state.selectedTab === 3) {
      if (
        this.state.selectedPage !== "" ||
        this.state.selectedPagesForDocLevel.length > 0 ||
        this.state.selectedPagesArray.length > 0
      ) {
        this.setState({
          nextButtonDisabled: false,
          disableTab5: false,
        });
      } else {
        this.setState({
          nextButtonDisabled: true,
          disableTab5: true,
          disableTab6: true,
        });
      }
    } else if (this.state.selectedTab === 4) {
      if (this.state.conditionsJson.length < 1) {
        this.setState({
          nextButtonDisabled: true,
          disableTab6: true,
        });
      } else {
        this.setState({
          nextButtonDisabled: false,
          disableTab6: false,
        });
      }
    }
  };

  validateModal = () => {
    if (
      this.state.selectedModalField !== "" &&
      this.state.selectedModalOperator !== "" &&
      this.state.selectedModalValue !== "" &&
      this.state.selectedModalAndOrBlank !== ""
    ) {
      return 1;
    } else {
      return 0;
    }
  };

  changeProgressBarClicked = (e) => {
    if (!this.state.isDevelopent) {
      return;
    }
    console.log("e", e);
    if (
      //   e === 0 ||
      (e === 2 && this.state.disableTab2 === true) ||
      (e === 3 && this.state.disableTab3 === true) ||
      (e === 4 && this.state.disableTab4 === true) ||
      (e === 5 && this.state.disableTab5 === true) ||
      (e === 6 && this.state.disableTab6 === true)
    ) {
      return;
    }
    this.setState(
      {
        selectedTab: e,
      },
      () => {
        this.validate();
      }
    );
  };

  changeTabClicked = (e) => {
    e.persist();
    console.log("e", e);
    var tabClicked = e.target.id;
    console.log("tabClicked", tabClicked);
    console.log("!isNaN(tabClicked)", !isNaN(tabClicked));
    if (isNaN(tabClicked)) {
      //to Check for Disabled Tabs

      tabClicked = tabClicked.slice(4, 5);
      tabClicked = parseInt(tabClicked) - 1;
      console.log("ChangeTabClicked", tabClicked);
      this.setState(
        {
          selectedTab: tabClicked,
        },
        () => {
          this.validate();
        }
      );
    }
  };

  changeDDApplication = (e) => {
    console.log("e", e);
    this.resetDocPageFieldTabs();
    this.setState(
      {
        selectedApplication: e.selectedItem,
      },
      () => {
        console.log("selected App", this.state.selectedApplication);
        this.validate();
        this.getDCOTree(this.state.selectedApplication);
      }
    );
  };

  resetDocPageFieldTabs = () => {
    this.resetDocumentTab();
    this.resetPageTab();
    this.resetFieldTab();
  };

  getDCOTree = (application) => {
    console.log("application", application);
    ecm.model.Request.invokePluginService(
      "SETemplateBuilderPlugin",
      "SETemplateBuilderPluginService",
      {
        requestParams: { methodName: "getDcoasJSON", application: application },
        requestCompleteCallback: (response) => {
          console.log("response ", response);
          this.setState({
            dcoTree: response,
            rbDocumentTypes: response["documents"],
          });
        },
        requestFailedCallback: function (error) {
          console.log("request failed with error : ", error);
        },
        requestHeaders: { "Cache-Control": "no-cache" },
      }
    );
    console.log("rbDocumentTypes", this.state.rbDocumentTypes);
  };

  changeDDLevel = (e) => {
    this.resetDocPageFieldTabs();
    this.setState(
      {
        selectedLevel: e.selectedItem,
      },
      () => {
        console.log("selected Level:", this.state.selectedLevel);
        console.log("rbDocumentTypes", this.state.rbDocumentTypes);
        this.validate();
      }
    );
  };

  changeDDAppendToFile = (e) => {
    this.setState(
      {
        selectedAppendToFile: e.selectedItem,
      },
      () => {
        console.log("Selected AppendToFile", this.state.selectedAppendToFile);
        this.validate();
      }
    );
  };
  changeDDLocale = (e) => {
    this.setState(
      {
        selectedLocale: e.selectedItem,
      },
      () => {
        console.log("Selected Locale", this.state.selectedLocale);
        this.validate();
      }
    );
  };
  changeDDExportFormat = (e) => {
    this.setState(
      {
        selectedExportFormat: e.selectedItem,
        ddDelimiter: this.state.delimiterJson[e.selectedItem],
      },
      () => {
        this.setState(
          {
            selectedDelimiter: "",
          },
          () => {
            this.validate();
            console.log(
              "Selected Export Format",
              this.state.selectedExportFormat
            );
            console.log(
              "Selected Export selectedDelimiter",
              this.state.selectedDelimiter
            );
          }
        );
      }
    );
  };
  changeDDDelimiter = (e) => {
    this.setState(
      {
        selectedDelimiter: e.selectedItem,
      },
      () => {
        console.log("Selected Delimiter", this.state.selectedDelimiter);
        this.validate();
      }
    );
  };

  changeFileName = (e) => {
    e.persist();
    this.setState(
      {
        selectedFileName: e.target.value,
      },
      () => {
        console.log("Selected FileName", this.state.selectedFileName);
        this.validate();
      }
    );
  };

  changeFileNameSmartP = (e) => {
    this.setState(
      {
        selectedFileNameSmartP: e.selectedItem,
      },
      () => {
        console.log(
          "Selected FileName smart p",
          this.state.selectedFileNameSmartP
        );
        this.validate();
      }
    );
  };
  downloadTemplate = (e) => {
    console.log("in downloadTemplate");
    var text = this.state.codeSnippet;
    console.log(text);
    var element = document.createElement("a");
    element.setAttribute(
      "href",
      "data:text/xml;charset=utf-8," + encodeURIComponent(text)
    );
    element.setAttribute("download", "SmartExportTemplate.xml");

    element.style.display = "none";
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
  };

  changeOutputFolder = (e) => {
    e.persist();
    this.setState(
      {
        selectedOutputFolder: e.target.value,
      },
      () => {
        console.log("Selected Output Folder", this.state.selectedOutputFolder);
        this.validate();
      }
    );
  };

  changeOutputFolderSmartP = (e) => {
    this.setState(
      {
        selectedOutputFolderSmartP: e.selectedItem,
      },
      () => {
        console.log(
          "Selected Output Folder smart param",
          this.state.selectedOutputFolderSmartP
        );
        this.validate();
      }
    );
  };

  resetPageFieldTabs = () => {
    this.resetPageTab();
    this.resetFieldTab();
  };

  changeRBDocumentTypes = (e) => {
    this.resetPageFieldTabs();
    console.log(e);
    this.setState(
      {
        selectedDocumentType: e,
        rbPages: this.state.dcoTree.documentPageMap[e],
      },
      () => {
        console.log("Selected Document Type", this.state.selectedDocumentType);
        this.validate();
      }
    );
  };

  changeCBDocumentTypes = (e) => {
    this.resetPageFieldTabs();
    if (e.target.checked) {
      this.state.selectedDocumentsForBatchLevel.push(e.target.value);
    } else {
      var index = this.state.selectedDocumentsForBatchLevel.findIndex(
        (item) => item === e.target.value
      );
      this.state.selectedDocumentsForBatchLevel.splice(index, 1);
    }
    console.log(
      "Selected Documents",
      this.state.selectedDocumentsForBatchLevel
    );
    this.validate();
  };
  checkEvent1 = (e) => {
    console.log(e);
  };

  checkEvent2 = (e) => {
    e.persist();
    console.log(e);
  };

  changeRBPages = (e) => {
	this.resetFieldTab();
    console.log(e);
    this.setState(
      {
        selectedPage: e,
        cbFields: this.state.dcoTree.pageFieldMap[e],
      },
      () => {
        console.log("Selected Page", this.state.selectedPage);
        this.validate();
      }
    );
  };
  changeCBPages = (e) => {
    this.resetFieldTab();
    if (this.state.selectedLevel === "Document") {
      if (e.target.checked) {
        this.state.selectedPagesForDocLevel.push(e.target.value);
      } else {
        var index = this.state.selectedPagesForDocLevel.findIndex(
          (item) => item === e.target.value
        );
        this.state.selectedPagesForDocLevel.splice(index, 1);
      }
      console.log("Selected Pages ", this.state.selectedPagesForDocLevel);
    } else if (this.state.selectedLevel === "Batch") {
      var pageList = [];
      if (
        this.state.selectedDocPageMap.has(this.state.currentSelectedDocForPages)
      ) {
        pageList = this.state.selectedDocPageMap.get(
          this.state.currentSelectedDocForPages
        );
      }
      if (e.target.checked) {
        pageList.push(e.target.value);
        this.state.selectedPagesArray.push(e.target.value);
      } else {
        var index = pageList.findIndex((item) => item === e.target.value);
        pageList.splice(index, 1);
        var index = this.state.selectedPagesArray.findIndex(
          (item) => item === e.target.value
        );
        this.state.selectedPagesArray.splice(index, 1);
      }
      this.state.selectedDocPageMap.set(
        this.state.currentSelectedDocForPages,
        pageList
      );
      console.log("Selected Pages Map", this.state.selectedDocPageMap);
      console.log("Selected Pages Array", this.state.selectedPagesArray);
    }
    this.validate();
  };

  changecbFields = (eventField, event) => {
    // e.persist();
    // if (this.state.selectedLevel === "Page") {
    var tempSelectedFields = this.state.selectedFields;
    //console.log(tempSelectedFields);
    var fieldObj = { field: { key: eventField.name, value: eventField.id } };
    if (event) {
      tempSelectedFields.push(fieldObj);
    } else {
      var index = tempSelectedFields.findIndex(
        (item) => item.field.value === eventField.id
      );
      console.log("index", index);
      tempSelectedFields.splice(index, 1);
    }
    this.setState(
      {
        selectedFields: tempSelectedFields,
      },
      () => {
        console.log("Selected Fields", this.state.selectedFields);
        this.validate();
      }
    );
    console.log(this.state.selectedFields);
    if (
      this.state.selectedLevel === "Document" ||
      this.state.selectedLevel === "Batch"
    ) {
      if (event) {
        this.state.selectedFieldIdsArray.push(eventField.id);
      } else {
        var selectedIndex = this.state.selectedFieldIdsArray.findIndex(
          (item) => item === eventField.id
        );
        this.state.selectedFieldIdsArray.splice(selectedIndex, 1);
      }
    }
    /* } else if (
      this.state.selectedLevel === "Document" ||
      this.state.selectedLevel === "Batch"
    ) {
      var fieldsArray = [];
      if (
        this.state.selectedFieldsForDocLevelMap.has(
          this.state.currentSelectedPageForFields
        )
      ) {
        fieldsArray = this.state.selectedFieldsForDocLevelMap.get(
          this.state.currentSelectedPageForFields
        );
      }
      var fieldObj = { field: { key: eventField.name, value: eventField.id } };
      if (event) {
        fieldsArray.push(fieldObj);
        this.state.selectedFieldIdsArray.push(eventField.id);
      } else {
        var index = fieldsArray.findIndex(
          (item) => item.field.value === eventField.id
        );
        //console.log("index", index)
        fieldsArray.splice(index, 1);
        var selectedIndex = this.state.selectedFieldIdsArray.findIndex(
          (item) => item === eventField.id
        );
        //console.log("selectedIndex", selectedIndex)
        this.state.selectedFieldIdsArray.splice(selectedIndex, 1);
      }
      this.state.selectedFieldsForDocLevelMap.set(
        this.state.currentSelectedPageForFields,
        fieldsArray
      );
      console.log(this.state.selectedFieldsForDocLevelMap);
      console.log(this.state.selectedFieldIdsArray);
    }*/
  };
  changecbFieldValue = (eventField, e) => {
    // console.log("e----",event.target.value);
    this.state.changedFieldsLabelsMap.set(eventField.id, e.target.value);
    console.log(this.state.changedFieldsLabelsMap);
  };

  getDefaultFieldvalue = (value) => {
    if (this.state.changedFieldsLabelsMap.has(value.id)) {
      return this.state.changedFieldsLabelsMap.get(value.id);
    } else {
      return value.name;
    }
  };

  updateSelectedFieldValues = () => {
    if (
      this.state.selectedLevel === "Page" ||
      this.state.selectedLevel === "Field"
    ) {
      if (this.state.conditionsJson.length > 0) {
        console.log("here in group update ");
      } else {
        var tempSelectedFields = this.state.selectedFields;
        tempSelectedFields.forEach((item) => {
          if (this.state.changedFieldsLabelsMap.has(item.field.value)) {
            item.field.key = this.state.changedFieldsLabelsMap.get(
              item.field.value
            );
          }
        });
        this.setState({
          selectedFields: tempSelectedFields,
        });
      }
    } else if (
      this.state.selectedLevel === "Document" ||
      this.state.selectedLevel === "Batch"
    ) {
      console.log(
        "this.state.selectedFieldsForDocLevelMap",
        this.state.selectedFieldsForDocLevelMap
      );
      this.state.selectedFieldsForDocLevelMap.forEach((fieldArray, page) => {
        fieldArray.forEach((item) => {
          if (this.state.changedFieldsLabelsMap.has(item.field.value)) {
            item.field.key = this.state.changedFieldsLabelsMap.get(
              item.field.value
            );
          }
        });
        this.state.selectedFieldsForDocLevelMap.set(page, fieldArray);
      });
      // console.log("this.state.selectedFieldsForDocLevelMap after",this.state.selectedFieldsForDocLevelMap);
      this.updatePageFieldInfoJson();
    }
  };

  updatePageFieldInfoJson = () => {
    this.state.selectedFieldsForDocLevelMap.forEach((fieldArray, page) => {
      if (fieldArray.length > 0) {
        var perPageInfo = {
          "page.type": page,
          conditionGroups: [],
          fields: fieldArray,
        };
        this.state.pageFieldInfoJson.push(perPageInfo);
      }
    });
    console.log(this.state.pageFieldInfoJson);
    if (this.state.selectedLevel === "Batch") {
      this.updateDocFieldInfoJson();
      console.log(this.state.docFieldInfoJson);
    }
  };

  changeTableCbSelectedRow = (e) => {
    // e.persist();
    console.log(e.target.value);
    this.setState({
      selectedTableCbSelect: e.target.value,
    });
  };

  modalSaveClicked = () => {
    console.log("modalSaveClicked");
    if (1) {
      console.log("rowData", this.state.rowData);
      var tempTableRowObj = {
        id: this.state.placeholderId,
        field: this.state.selectedModalField,
        condition:
          this.state.selectedModalOperator +
          " " +
          this.state.selectedModalValue +
          " " +
          this.state.selectedModalAndOrBlank,
      };
      console.log("tempTableRowObj", tempTableRowObj);
      //   tempTableRowData.push(tempTableRowObj);
      //   console.log(tempTableRowData);
      this.setState(
        {
          rowData: [...this.state.rowData, tempTableRowObj],
          placeholderId: this.state.placeholderId + 1,
        },
        () => {
          console.log("rowData", this.state.rowData);
        }
      );
    }
  };

  changeModalDDGroups = (e) => {
    console.log(e.selectedItem);
    this.setState(
      {
        selectedModalGroup: e.selectedItem,
      },
      () => {
        console.log("Selected ", this.state.selectedModalGroup);
        this.validate();
      }
    );
  };

  changeModalDDOperators = (e) => {
    console.log(e.selectedItem);
    this.setState(
      {
        selectedModalOperator: e.selectedItem,
      },
      () => {
        console.log("Selected ", this.state.selectedModalOperator);
        this.validate();
      }
    );
  };

  changeModalTBValue = (e) => {
    e.persist();
    console.log(e.target.value);
    this.setState(
      {
        selectedModalValue: e.target.value,
      },
      () => {
        console.log("Selected ", this.state.selectedModalValue);
        this.validate();
      }
    );
  };

  changeModalDDAddOrBlank = (e) => {
    console.log(e.selectedItem);
    this.setState(
      {
        selectedModalAndOrBlank: e.selectedItem,
      },
      () => {
        console.log("Selected ", this.state.selectedModalAndOrBlank);
        this.validate();
      }
    );
  };

  getFieldLevelInputJson = () => {
    return {
      application: this.state.selectedApplication,
      level: this.state.selectedLevel,
      appendToFile: this.state.selectedAppendToFile,
      locale: {
        value: this.state.selectedLocale,
        smartparam: "",
      },
      fileName: {
        value: this.state.selectedFileName,
        smartparam: this.state.selectedFileNameSmartP,
      },
      exportFormat: this.state.selectedExportFormat,
      outputFolder: {
        value: this.state.selectedOutputFolder,
        smartparam: this.state.selectedOutputFolderSmartP,
      },

      delimeter: this.state.selectedDelimiter,
      fields: this.state.selectedFields,
    };
  };

  getPageLevelInputJson = () => {
    return {
      application: this.state.selectedApplication,
      level: this.state.selectedLevel,
      appendToFile: this.state.selectedAppendToFile,
      locale: {
        value: this.state.selectedLocale,
        smartparam: "",
      },
      fileName: {
        value: this.state.selectedFileName,
        smartparam: this.state.selectedFileNameSmartP,
      },
      exportFormat: this.state.selectedExportFormat,
      outputFolder: {
        value: this.state.selectedOutputFolder,
        smartparam: this.state.selectedOutputFolderSmartP,
      },

      delimeter: this.state.selectedDelimiter,
      "page.type": this.state.selectedPage,
      conditionGroups: this.state.conditionsJson,
      //fields: this.state.selectedFields,
    };
  };

  getDocumentLevelInputJson = () => {
    return {
      application: this.state.selectedApplication,
      level: this.state.selectedLevel,
      appendToFile: this.state.selectedAppendToFile,
      locale: {
        value: this.state.selectedLocale,
        smartparam: "",
      },
      fileName: {
        value: this.state.selectedFileName,
        smartparam: this.state.selectedFileNameSmartP,
      },
      exportFormat: this.state.selectedExportFormat,
      outputFolder: {
        value: this.state.selectedOutputFolder,
        smartparam: this.state.selectedOutputFolderSmartP,
      },

      delimeter: this.state.selectedDelimiter,
      "document.type": this.state.selectedDocumentType,
      pageFieldInfo: this.state.pageFieldInfoJson,
    };
  };

  updateDocFieldInfoJson = () => {
    var outputmap = new Map();
    this.state.pageFieldInfoJson.forEach((pageObj) => {
      this.state.selectedDocPageMap.forEach((pageList, docName) => {
        var pageJsonList = [];
        if (outputmap.has(docName)) {
          pageJsonList = outputmap.get(docName);
        }
        if (pageList.includes(pageObj["page.type"])) {
          pageJsonList.push(pageObj);
        }
        outputmap.set(docName, pageJsonList);
      });
    });
    outputmap.forEach((pagelist, doc) => {
      var json = {
        "document.type": doc,
        pageFieldInfo: pagelist,
      };
      this.state.docFieldInfoJson.push(json);
    });
  };

  getBatchLevelInputJson = () => {
    return {
      application: this.state.selectedApplication,
      level: this.state.selectedLevel,
      appendToFile: this.state.selectedAppendToFile,
      locale: {
        value: this.state.selectedLocale,
        smartparam: "",
      },
      fileName: {
        value: this.state.selectedFileName,
        smartparam: this.state.selectedFileNameSmartP,
      },
      exportFormat: this.state.selectedExportFormat,
      outputFolder: {
        value: this.state.selectedOutputFolder,
        smartparam: this.state.selectedOutputFolderSmartP,
      },
      delimeter: this.state.selectedDelimiter,
      documentPageInfo: this.state.docFieldInfoJson,
    };
  };

  getSETemplate = () => {
    var input = {
      application: this.state.selectedApplication,
      level: this.state.selectedLevel,
      appendToFile: this.state.selectedAppendToFile,
      locale: {
        value: this.state.selectedLocale,
        smartparam: "",
      },
      fileName: {
        value: this.state.selectedFileName,
        smartparam: this.state.selectedFileNameSmartP,
      },
      exportFormat: this.state.selectedExportFormat,
      outputFolder: {
        value: this.state.selectedOutputFolder,
        smartparam: this.state.selectedOutputFolderSmartP,
      },

      delimeter: this.state.selectedDelimiter,
      conditionGroups: this.state.conditionsJson,
    };

    /*if (this.state.selectedLevel === "Page") {
      input = this.getPageLevelInputJson();
    } else if (this.state.selectedLevel === "Document") {
      input = this.getDocumentLevelInputJson();
    } else if (this.state.selectedLevel === "Batch") {
      input = this.getBatchLevelInputJson();
    } else if (this.state.selectedLevel === "Field") {
      input = this.getFieldLevelInputJson();
    }*/

    console.log("input json", input);

    ecm.model.Request.postPluginService(
      "SETemplateBuilderPlugin",
      "SETemplateBuilderPluginService",
      "text/json",
      {
        requestParams: { methodName: "getSETemplate" },
        requestBody: input,
        requestCompleteCallback: (response) => {
          console.log("response setem======", response);
          this.setState({
            codeSnippet: response.seTemplate,
          });
        },
        requestFailedCallback: (error) => {
          console.log("request failed with error : ", error);
        },
        requestHeaders: { "Cache-Control": "no-cache" },
      }
    );
  };

  clickedAddGroupAction = (e) => {
    console.log("e", e);
    var groupName = "Group " + this.state.groupIndex;
    this.setState({
      ddGroups: [...this.state.ddGroups, groupName],
      groupIndex: this.state.groupIndex + 1,
    });
    // console.log("hie");
    // if (1) {
    //   var tempTableRowData = this.state.rowData;
    //   console.log(tempTableRowData);
    //   //   var tempTableRowObj = {
    //   //     id: "1",
    //   //     group: "Group1",
    //   //     field: this.state.selectedModalField,
    //   //     condition:
    //   //       this.state.selectedModalOperator +
    //   //       " " +
    //   //       this.state.selectedModalValue,
    //   //   };
    //   var tempTableRowObj = {
    //     id: "5",
    //     field: "field4",
    //     condition: "condition4",
    //   };
    //   tempTableRowData.push(tempTableRowObj);
    //   console.log(tempTableRowData);
    //   this.setState({
    //     rowData: tempTableRowData,
    //   });
    // }
  };
  clickedRemoveRow(e) {
    console.log("e", e);
  }

  clickedEditRow(e) {
    console.log("e", e);
  }

  printText() {
    console.log("PrintText");
  }

  addDataToDataTable = () => {
    console.log("hie");
    if (1) {
      var tempTableRowData = this.state.rowData;
      console.log(tempTableRowData);
      var tempTableRowObj = {
        id: "1",
        group: "Group1",
        field: this.state.selectedModalField,
        condition:
          this.state.selectedModalOperator +
          " " +
          this.state.selectedModalValue,
      };
      tempTableRowData.push(tempTableRowObj);
      console.log(tempTableRowData);
      this.setState({
        rowData: tempTableRowData,
      });
    }
  };

  renderDocumentTabForPageAndDocLevel = () => {
    return (
      <div>
        <FormGroup legendText="There are multiple document types associated with this application. Please select the Document Type from which the data needs to be exported.">
          <RadioButtonGroup
            legend="Group Legend"
            name="radio-button-group-documenttype"
            onChange={this.changeRBDocumentTypes}
          >
            {this.state.rbDocumentTypes &&
              this.state.rbDocumentTypes.map((value, index) => {
                return (
                  <RadioButton
                    labelText={value.name}
                    value={value.id}
                    key={value.id}
                  />
                );
              })}
          </RadioButtonGroup>
        </FormGroup>
      </div>
    );
  };

  renderDocumentTabForBatchLevel = () => {
    return (
      <div>
        <FormGroup legendText="There are multiple document types associated with this application. Please select the Document Types for which the data needs to be exported.">
          <fieldset
            className="bx--fieldset"
            onChange={this.changeCBDocumentTypes}
          >
            {this.state.rbDocumentTypes.map((value, index) => {
              return (
                <div className="document-grid bx--grid">
                  <div className="bx--row">
                    <Checkbox
                      labelText={value.name}
                      value={value.name}
                      id={"checked-" + value.id}
                      key={value.id}
                    />
                  </div>
                </div>
              );
            })}
          </fieldset>
        </FormGroup>
      </div>
    );
  };

  setSelectedFieldAtFieldLvl = (eventField, e) => {
    var tempSelectedFields = [];
    var tempRbFields = [];
    var fieldObj = { field: { key: eventField.name, value: eventField.id } };
    if (e) {
      tempSelectedFields.push(fieldObj);
      var rbFieldObj = { id: eventField.id, name: eventField.name };
      tempRbFields.push(rbFieldObj);
    }
    this.setState(
      {
        selectedFields: tempSelectedFields,
        rbFields: tempRbFields,
      },
      () => {
        console.log("field level ", this.state.selectedFields);
      }
    );
  };

  renderPageTabForPageLevel = () => {
    return (
      <div>
        <FormGroup
          legendText="There are multiple pages associated to this document type.
                         Please select the page from which the data needs to be exported."
        >
          <RadioButtonGroup
            //defaultSelected={this.state.rbPages[0].id}
            legend="Group Legend2"
            name="radio-button-group-pages"
            valueSelected={this.state.selectedPage}
            onChange={this.changeRBPages}
          >
            {this.state.rbPages.map((value, index) => {
              return (
                <RadioButton
                  labelText={value.name}
                  value={value.id}
                  key={value.id}
                />
              );
            })}
          </RadioButtonGroup>
        </FormGroup>
      </div>
    );
  };

  renderPageTabForDocumentLevel = () => {
    return (
      <div>
        <fieldset className="bx--fieldset" onChange={this.changeCBPages}>
          {this.state.selectedLevel === "Document" && (
            <legend className="bx--label">
              There are multiple pages associated to this document type. Please
              select the pages from which the data needs to be filtered.
            </legend>
          )}
          {this.state.rbPages.map((value, index) => {
            return (
              <div className="document-grid bx--grid">
                <div className="bx--row">
                  <Checkbox
                    labelText={value.name}
                    value={value.name}
                    id={"checked-" + value.id}
                    key={value.id}
                    defaultChecked={this.isPageSelected(value.name)}
                  />
                </div>
              </div>
            );
          })}
        </fieldset>
      </div>
    );
  };

  renderPageTabForBatchLevel = () => {
    return (
      <div>
        <div>
          <legend className="bx--label">
            For each document type, please select the associated pages from
            which the fields need to be exported.
          </legend>
          <Select
            id="selectDocument"
            defaultValue="{this.state.currentSelectedDocForPages}"
            inline={false}
            labelText="Document type :"
            light={false}
            onChange={(e) => this.handleSelectDocument(e)}
            size="sm"
          >
            {this.state.selectedDocumentsForBatchLevel.map((value, index) => {
              return <SelectItem text={value} value={value} />;
            })}
          </Select>
        </div>
        <div>{this.renderPageTabForDocumentLevel()}</div>
      </div>
    );
  };

  renderFieldsTabForFieldLevel = () => {
    return (
      <fieldset className="bx--fieldset">
        <legend className="bx--label">
          Please select the field that needs to be exported. If a Field needs to
          be exported on satisfying specific conditions, add that field to a
          group and assocciate the group with the required condition(s).
        </legend>
        <div className="document-grid bx--grid">
          <div className="bx--row">
            <div className="bx--col-lg-6">
              <FormLabel>DCO Element Name</FormLabel>
            </div>
            <div className="bx--col-lg-6">
              <FormLabel>Element Name on Export File</FormLabel>
            </div>
          </div>
        </div>
        {this.state.cbFields.map((value, index) => {
          return (
            <div className="document-grid bx--grid">
              <FormGroup legendText="">
                <div className="bx--row">
                  <div className="bx--col-lg-6 bx--form-item">
                    <RadioButton
                      labelText={value.name}
                      value={value.id}
                      key={value.id}
                      name="fieldlevelradio"
                      id={"radiofield-" + value.id}
                      disabled={this.isFieldRbDisabled()}
                      onChange={(e) =>
                        this.setSelectedFieldAtFieldLvl(value, e)
                      }
                    />
                  </div>
                  <div className="bx--col-lg-6 label-hide">
                    <TextInput
                      id={"fieldinput-" + value.id}
                      invalidText="A valid value is required"
                      labelText="Field Name"
                      placeholder="Field Name"
                      size="sm"
                      key={value.id}
                      defaultValue={this.getDefaultFieldvalue(value)}
                      type="text"
                      onBlur={(e) => this.changecbFieldValue(value, e)}
                    />
                  </div>
                </div>
              </FormGroup>
            </div>
          );
        })}
      </fieldset>
    );
  };
  renderFieldsTabForPageLevel = () => {
    return (
      <fieldset className="bx--fieldset">
        {this.state.selectedLevel === "Page" && (
          <legend className="bx--label">
            Please select the fields that needs to be exported. If a Field or
            Group of Fileds need to be exported on satisfying specific
            conditions, add them to a group and assocciate the group with the
            required condition(s).
          </legend>
        )}
        <div className="document-grid bx--grid">
          <div className="bx--row">
            <div className="bx--col-lg-6">
              <FormLabel>DCO Element Name</FormLabel>
            </div>
            <div className="bx--col-lg-6">
              <FormLabel>Element Name on Export File</FormLabel>
            </div>
          </div>
        </div>
        {this.state.cbFields.map((value, index) => {
          return (
            <div className="document-grid bx--grid">
              <div className="bx--row">
                <div className="bx--col-lg-6">
                  <Checkbox
                    labelText={value.name}
                    value={value.name}
                    id={"checked-" + value.id}
                    key={value.id}
                    defaultChecked={this.isFieldSelected(value.id)}
                    onChange={(e) => this.changecbFields(value, e)}
                  />
                </div>
                <div className="bx--col-lg-6 label-hide">
                  <TextInput
                    id={"input-" + value.id}
                    invalidText="A valid value is required"
                    labelText="Field Name"
                    placeholder="Field Name"
                    size="sm"
                    key={value.id}
                    defaultValue={this.getDefaultFieldvalue(value)}
                    type="text"
                    onBlur={(e) => this.changecbFieldValue(value, e)}
                  />
                </div>
              </div>
            </div>
          );
        })}
      </fieldset>
    );
  };
  renderFieldsTabForDocumentLevel = () => {
    return (
      <div>
        <div>
          {this.state.selectedLevel === "Document" && (
            <legend className="bx--label">
              For each page type, please select the fields that needs to be
              exported If a Field or group of fields needs to be exported on
              satisfying specific conditions, add them to a group and associate
              the group with the required condition(s).
            </legend>
          )}
          <Select
            id="selectPage"
            defaultValue="{this.state.currentSelectedPageForFields}"
            inline={false}
            labelText="Page Type :"
            light={false}
            onChange={(e) => this.handleSelectPage(e)}
            size="sm"
          >
            {this.state.selectedPagesForDocLevel.map((value, index) => {
              return <SelectItem text={value} value={value} />;
            })}
          </Select>
        </div>
        <div>{this.renderFieldsTabForPageLevel()}</div>
      </div>
    );
  };

  renderFieldsTabForBatchLevel = () => {
    return (
      <div>
        <div>
          <legend className="bx--label">
            For each document type and its associated page types, please select
            the fields that needs to be exported. If a Field or group of fields
            need to be exported on satisfying specific conditions, add them to a
            group and associate the group with the required condition().
          </legend>
          <Select
            id="selectDoc"
            defaultValue="{this.state.currentSelectedDocInFieldTab}"
            inline={false}
            labelText="Document Type :"
            light={false}
            onChange={(e) => this.handleSelectDocumentInFieldTab(e)}
            size="sm"
          >
            {this.state.selectedDocumentsForBatchLevel.map((doc, index) => {
              return <SelectItem text={doc} value={doc} />;
            })}
          </Select>
        </div>
        <div>{this.renderFieldsTabForDocumentLevel()}</div>
      </div>
    );
  };

  handleSelectPage = (event) => {
    console.log("e----", event.target.value);
    this.setState({
      currentSelectedPageForFields: event.target.value,
      cbFields: this.state.dcoTree.pageFieldMap[event.target.value],
    });
  };

  handleSelectDocument = (event) => {
    console.log("e----", event.target.value);
    this.setState({
      currentSelectedDocForPages: event.target.value,
      rbPages: this.state.dcoTree.documentPageMap[event.target.value],
    });
  };

  handleSelectDocumentInFieldTab = (event) => {
    console.log("e----", event.target.value);
    this.setState({
      currentSelectedDocInfieldTab: event.target.value,
      selectedPagesForDocLevel: this.state.selectedDocPageMap.get(
        event.target.value
      ),
      currentSelectedPageForFields: this.state.selectedDocPageMap.get(
        event.target.value
      )[0],
      cbFields: this.state.dcoTree.pageFieldMap[
        this.state.selectedDocPageMap.get(event.target.value)[0]
      ],
    });
    console.log(
      "selectedPagesForDocLevel in handle",
      this.state.selectedPagesForDocLevel
    );
  };

  isFieldSelected = (fieldId) => {
    return this.state.selectedFieldIdsArray.includes(fieldId);
  };

  isPageSelected = (pageName) => {
    return this.state.selectedPagesArray.includes(pageName);
  };

  uiProgressBar = () => {
    return (
      <ProgressIndicator
        currentIndex={this.state.selectedTab}
        onChange={this.changeProgressBarClicked}
        vertical={false}
        index={this.state.selectedTab}
      >
        {/* <div className="class-info">
          <div>
            <InformationFilled32 onClick={this.openInfoModal} />
          </div>
        </div> */}
        <ProgressStep
          label="Application Details"
          //   renderLabel={function noRefCheck() {}}
          //   secondaryLabel="Optional label"
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab1}
          //   invalid={this.state.disableTab1}
        />
        <ProgressStep
          label="Export Details"
          //   renderLabel={function noRefCheck() {}}
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab2}
          //   invalid={this.state.invalidTab2}
        />
        <ProgressStep
          label="Document Type"
          //   renderLabel={function noRefCheck() {}}
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab3}
          //   invalid={this.state.disableTab3 && (this.state.selectedTab>=2)}
        />
        <ProgressStep
          label="Page Type"
          //   renderLabel={function noRefCheck() {}}
          //   secondaryLabel="Example invalid step"
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab4}
          //   invalid={this.state.invalidTab4}
        />
        <ProgressStep
          label="Field Type"
          //   renderLabel={function noRefCheck() {}}
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab5}
          //   invalid={this.state.invalidTab5}
        />
        <ProgressStep
          label="Review & Download"
          //   renderLabel={function noRefCheck() {}}
          translateWithId={function noRefCheck() {}}
          disabled={this.state.disableTab6}
          //   invalid={this.state.invalidTab6}
        />
      </ProgressIndicator>
    );
  };

  openInfoModal = () => {
    this.setState({
      isInfoModalOpen: !this.state.isInfoModalOpen,
    });
  };

  closeInfoModal = () => {
    this.setState({
      isInfoModalOpen: false,
    });
  };

  render() {
    return (
      <div>
        <div className="cursor" style={{ cursor: "default!important" }}>
          {this.uiProgressBar()}
        </div>
        {/* <div>
          <Modal
            iconDescription="Close"
            modalHeading="Information"
            modalLabel=""
            passiveModal={true}
            size="sm"
            open={this.state.isInfoModalOpen}
            onRequestClose={this.closeInfoModal}
          >
            <br />
            <h3>
              Using the <b>Smart Export Interface</b>, Business users can
              indicate specific data elements from the document, of business
              importance, that needs to be selectively exported by Datacap
              Application.
            </h3>
            <br />
            <p>The selective Export can be assocciated at multiple levels:-</p>
            <OrderedList>
              <ListItem>
                <b>Field Level :</b> To export a specific Field from the
                document.
              </ListItem>
              <ListItem>
                <b>Page Level :</b> To export multiple Fields within a page
                type.
              </ListItem>
              <ListItem>
                <b>Document Level :</b> To export Fields from multiple page
                types within a document type.
              </ListItem>
              <ListItem>
                <b>Batch Level :</b> To export Fields from multiple page types
                across multiple document types.
              </ListItem>
            </OrderedList>
          </Modal>
        </div> */}

        <div>
          <Tabs
            selected={this.state.selectedTab}
            onClick={this.changeTabClicked}
            type="container"
          >
            <Tab
              disabled={this.state.disableTab1}
              href="#"
              id="tab-1"
              label="Applcation Details"
            >
              <div style={{ display: "flex", "min-height": "90px" }}>
                <div style={{ width: "50%" }}>
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-datacap"
                    items={this.state.ddDatacapApp}
                    label="--Please Select--"
                    titleText="Please select the Datacap Application for selective export."
                    onChange={this.changeDDApplication}
                    type="inline"
                    size="sm"
                  />
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-level"
                    items={this.state.ddLevel}
                    label="--Please Select--"
                    titleText="Please select the level of export."
                    onChange={this.changeDDLevel}
                    type="inline"
                    size="sm"
                  />
                </div>
                <div style={{ width: "50%", display: "flex" }}>
                  <div style={{ width: "5%" }} className="class-info">
                    <div title="Info">
                      <InformationFilled32 onClick={this.openInfoModal} />
                    </div>
                  </div>
                  {this.state.isInfoModalOpen && (
                    <div className="info-div">
                      <div>
                        <OrderedList>
                          <ListItem>
                            <b>Field Level :</b> To export a specific Field from
                            the page.
                          </ListItem>
                          <ListItem>
                            <b>Page Level :</b> To export multiple Fields within
                            a page type.
                          </ListItem>
                          <ListItem>
                            <b>Document Level :</b> To export Fields from
                            multiple page types within a document type.
                          </ListItem>
                          <ListItem>
                            <b>Batch Level :</b> To export Fields of multiple
                            page types across multiple document types.
                          </ListItem>
                        </OrderedList>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </Tab>
            <Tab
              disabled={this.state.disableTab2}
              href="#"
              id="tab-2"
              label="Output File Details"
            >
              <div className="tab-content">
                <div>
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-append-tof"
                    items={this.state.ddTrueFalse}
                    label="--Please Select--"
                    titleText="Do you want the data exported accross documents to be appended to same file?"
                    onChange={this.changeDDAppendToFile}
                    selected={this.state.selectedApplication}
                    type="inline"
                    size="sm"
                  />
                </div>
                <div>
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-locale"
                    items={this.state.ddLocale}
                    label="--Please Select--"
                    titleText="File Locale"
                    onChange={this.changeDDLocale}
                    type="inline"
                    size="sm"
                  />
                </div>
                {/* <div className="document-grid bx--grid smart-param">
                  <div className="bx--row">
                    <div className="bx--col-lg-1">
                      <FormLabel>Locale?</FormLabel>
                    </div>
                    <div className="bx--col-lg-5">
                      <Dropdown
                        ariaLabel="Dropdown"
                        id="carbon-dropdown-locale"
                        items={this.state.ddLocale}
                        label="--Please Select--"
                        titleText="Locale?"
                        onChange={this.changeDDLocale}
                        // type="inline"
                        // size="sm"
                      />
                    </div>
                  </div>
                </div> */}

                <div className="document-grid bx--grid smart-param">
                  <div className="bx--row">
                    <div className="bx--col-lg-2">
                      <FormLabel>File Name</FormLabel>
                    </div>
                    <div className="bx--col-lg-5">
                      <FormGroup>
                        <TextInput
                          id="smartparam-filename"
                          invalidText="A valid value is required"
                          labelText="File Name"
                          placeholder="Export File Name"
                          // onChange={this.changeFileName}
                          onBlur={this.changeFileName}
                          size="sm"
                        />
                      </FormGroup>
                    </div>
                    <div className="bx--col-lg-5">
                      <Dropdown
                        ariaLabel="Dropdown"
                        id="carbon-dropdown-smartparam-filename"
                        items={this.state.ddSmartParam}
                        label="--Smart Param--"
                        titleText="Smart Param"
                        onChange={this.changeFileNameSmartP}
                        size="sm"
                      />
                    </div>
                  </div>
                </div>

                {/* <FormGroup>
                  <TextInput
                    id="fileName"
                    invalidText="A valid value is required"
                    //labelText="File Name"
                    placeholder="File Name"
                    // onChange={this.changeFileName}
                    onBlur={this.changeFileName}
                  />
                </FormGroup> */}

                <div className="document-grid bx--grid smart-param">
                  <div className="bx--row">
                    <div className="bx--col-lg-2">
                      <FormLabel>Output Folder</FormLabel>
                    </div>
                    <div className="bx--col-lg-5">
                      <FormGroup>
                        <TextInput
                          id="smartparam-outputFolder"
                          invalidText="A valid value is required"
                          labelText="File Output Folder"
                          placeholder="Export File Output Folder"
                          onBlur={this.changeOutputFolder}
                          size="sm"
                        />
                      </FormGroup>
                    </div>
                    <div className="bx--col-lg-5">
                      <Dropdown
                        ariaLabel="Dropdown"
                        id="carbon-dropdown-smartparam-output"
                        items={this.state.ddSmartParam}
                        label="--Smart Param--"
                        titleText="Smart Param"
                        onChange={this.changeOutputFolderSmartP}
                        size="sm"
                      />
                    </div>
                  </div>
                </div>

                {/* <FormGroup>
                  <TextInput
                    id="outputFolder"
                    invalidText="A valid value is required"
                    //labelText="Output Folder"
                    placeholder="Output Folder"
                    // onChange={this.changeOutputFolder}
                    onBlur={this.changeOutputFolder}
                  />
                </FormGroup> */}

                <div>
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-exportformat"
                    items={this.state.ddExportFormat}
                    label="--Please Select--"
                    titleText="File Format"
                    onChange={this.changeDDExportFormat}
                    type="inline"
                    size="sm"
                  />
                </div>
                <div>
                  <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-delimiter"
                    selectedItem={this.state.selectedDelimiter}
                    items={this.state.ddDelimiter}
                    label="--Please Select--"
                    titleText="File Delimiter"
                    onChange={this.changeDDDelimiter}
                    type="inline"
                    size="sm"
                  />
                </div>
              </div>
            </Tab>
            <Tab
              disabled={this.state.disableTab3}
              href="#"
              id="tab-3"
              label="Document Level Details"
            >
              <div className="tab-content">
                {this.state.selectedLevel === "Field" &&
                  this.renderDocumentTabForPageAndDocLevel()}
                {this.state.selectedLevel === "Page" &&
                  this.renderDocumentTabForPageAndDocLevel()}
                {this.state.selectedLevel === "Document" &&
                  this.renderDocumentTabForPageAndDocLevel()}
                {this.state.selectedLevel === "Batch" &&
                  this.renderDocumentTabForBatchLevel()}
              </div>
            </Tab>
            <Tab
              disabled={this.state.disableTab4}
              href="#"
              id="tab-4"
              label="Page Level Details"
            >
              <div className="tab-content">
                {this.state.selectedLevel === "Field" &&
                  this.renderPageTabForPageLevel()}
                {this.state.selectedLevel === "Page" &&
                  this.renderPageTabForPageLevel()}
                {this.state.selectedLevel === "Document" &&
                  this.renderPageTabForDocumentLevel()}
                {this.state.selectedLevel === "Batch" &&
                  this.renderPageTabForBatchLevel()}
              </div>
            </Tab>
            <Tab
              disabled={this.state.disableTab5}
              href="#"
              id="tab-5"
              label="Field Level Details"
            >
              {" "}
              <div style={{ width: "100%" }}>
                <div style={{ width: "50%" }}>
                  {this.state.selectedLevel === "Field" &&
                    this.renderFieldsTabForFieldLevel()}
                  {this.state.selectedLevel === "Page" &&
                    this.renderFieldsTabForPageLevel()}
                  {this.state.selectedLevel === "Document" &&
                    this.renderFieldsTabForDocumentLevel()}
                  {this.state.selectedLevel === "Batch" &&
                    this.renderFieldsTabForBatchLevel()}
                </div>
                <div>
                  <Button
                    small
                    disabled={
                      this.state.selectedFields.length < 1 ||
                      (this.state.selectedLevel === "Field" &&
                        this.state.rowData3.length === 1)
                    }
                    onClick={this.AddToDTRows}
                  >
                    Add Groups
                  </Button>
                </div>
                {this.uiDataTable2()}
                {this.uiConditionBox()}
              </div>
            </Tab>
            <Tab
              disabled={this.state.disableTab6}
              href="#"
              id="tab-6"
              label="Validate & Download"
            >
              <div className="template-content">
                <FormLabel>Review and Download the XML </FormLabel>
                <div>
                  <CodeSnippet
                    feedback="Copied to clipboard"
                    onClick={function noRefCheck() {}}
                    showLessText="Show less"
                    showMoreText="Show more"
                    type="multi"
                  >
                    {this.state.codeSnippet}
                  </CodeSnippet>
                  <Button
                    small
                    className="btn-download"
                    onClick={this.downloadTemplate}
                  >
                    Download XML
                  </Button>
                  <Button
                    small
                    className="btn-prv2"
                    onClick={this.clickedPrevious}
                  >
                    &#60;&#60; Previous
                  </Button>
                </div>
              </div>
            </Tab>
            {/* <Tab
              disabled={this.state.disableTab5}
              href="#"
              id="tab-7"
              label="Level 7"
            >

            </Tab> */}
          </Tabs>
          {this.state.selectedTab === 4 && (
            <div style={{ width: "98%" }}>
              {this.uiPreviousButton()}
              {this.uiNextButton()}
            </div>
          )}
          {this.state.selectedTab !== 4 && this.state.selectedTab !== 5 && (
            <div style={{ width: "50%" }}>
              {this.uiPreviousButton()}
              {this.uiNextButton()}
            </div>
          )}
        </div>
      </div>
    );
  }

  addRow = () => {
    //   if(this.state.arrayConditions.length>0){
    //       return;
    //   }
    console.log("addRow");
    var fields = [];
    if (this.state.selectedLevel === "Field") {
      fields = this.state.rbFields;
    } else {
      fields = this.state.cbFields;
    }
    var objRow = {
      id: this.state.rowId,
      field: fields[0].id,
      fieldName: fields[0].name,
      operator: "",
      value: "",
      joinOperator: "",
    };

    console.log("objRow", objRow);
    this.setState(
      {
        rowId: this.state.rowId + 1,
        arrayConditions: [...this.state.arrayConditions, objRow],
      },
      () => {
        console.log("this.state.arrayConditions", this.state.arrayConditions);
      }
    );
  };

  //   changeConditionField = (e, index) => {
  //     console.log(e, index);

  //     var tempArray = [...this.state.arrayConditions];
  //     tempArray[index].field = e.selectedItem;
  //     this.setState({
  //       arrayConditions: [...tempArray],
  //     });
  //     console.log("this.state.arrayConditions", this.state.arrayConditions);
  //   };

  changeConditionField = (e, index) => {
    console.log(
      "e.target.options[e.target.selectedIndex].text",
      e.target.options[e.target.selectedIndex].text
    );
    console.log("Inside changeConditionField e.target.value", e.target.value);
    e.persist();
    console.log(e.target.value, index);

    var tempArray = [...this.state.arrayConditions];
    tempArray[index].field = e.target.value;
    tempArray[index].fieldName = e.target.options[e.target.selectedIndex].text;
    this.setState({
      arrayConditions: [...tempArray],
    });
    console.log("this.state.arrayConditions", this.state.arrayConditions);
  };

  changeConditionOperator = (e, index) => {
    console.log(e, index);

    var tempArray = [...this.state.arrayConditions];
    tempArray[index].operator = e.selectedItem;
    this.setState({
      arrayConditions: [...tempArray],
    });
    console.log("this.state.arrayConditions", this.state.arrayConditions);
  };

  changeConditionValue = (e, index) => {
    e.persist();
    console.log(e.target.value);

    var tempArray = [...this.state.arrayConditions];
    tempArray[index].value = e.target.value;
    this.setState({
      arrayConditions: [...tempArray],
    });
    console.log("this.state.arrayConditions", this.state.arrayConditions);
  };
  changeConditionJoin = (e, index) => {
    console.log(e, index);

    var tempArray = [...this.state.arrayConditions];
    tempArray[index].joinOperator = e.selectedItem;
    this.setState(
      {
        arrayConditions: [...tempArray],
      },
      () => {
        if (
          this.state.arrayConditions.length === index + 1 &&
          (e.selectedItem === "AND" || e.selectedItem === "OR")
        ) {
          this.addRow();
          console.log("this.state.arrayConditions", this.state.arrayConditions);
        } else if (e.selectedItem === "Blank") {
          var tempArray = this.state.arrayConditions.splice(0, index + 1);
          console.log("tempArray", tempArray);
          this.setState({ arrayConditions: [...tempArray] });
        }
      }
    );
  };

  uiConditionBox = () => {
    var conditionFields = [];
    if (this.state.selectedLevel === "Field") {
      conditionFields = this.state.rbFields;
    } else {
      conditionFields = this.state.cbFields;
    }
    if (this.state.arrayConditions.length) {
      return (
        <div>
          <div
            className="document-grid bx--grid condition-box"
            style={{ background: "var(--cds-field-01, #f4f4f4)" }}
          >
            <FormLabel>
              <b>Add/Edit Condition(s)</b>
            </FormLabel>
            <hr />
            {this.state.arrayConditions.map((value, index) => {
              return (
                <div className="bx--row">
                  <div className="bx--col-lg-3">
                    {/* <Dropdown
                    ariaLabel="Dropdown"
                    id="carbon-dropdown-modal-fields"
                    items={this.state.cbFields.map((item) => {
                      return item.value;
                    })}
                    label="--Please Select--"
                    titleText="Field"
                    onChange={(e) => this.changeConditionField(e, index)}
                    size="sm"
                    initialSelectedItem={value.field}
                  /> */}
                    <Select
                      value={value.field}
                      id={"conditionfield" + index}
                      invalidText="A valid value is required"
                      labelText="Field"
                      onChange={(e) => this.changeConditionField(e, index)}
                      size="sm"
                    >
                      {conditionFields.map((item, index) => {
                        return (
                          <SelectItem
                            text={item.name}
                            value={item.id}
                            id={"selectedDropDown-" + item.id}
                          />
                        );
                      })}
                    </Select>
                  </div>
                  <div className="bx--col-lg-3">
                    <Dropdown
                      ariaLabel="Dropdown"
                      id={"conditionoperator" + index}
                      items={this.state.ddOperators}
                      label="--Please Select--"
                      titleText="Operator"
                      onChange={(e) => this.changeConditionOperator(e, index)}
                      size="sm"
                      selectedItem={value.operator}
                    />
                  </div>
                  <div className="bx--col-lg-3" id="conditionbox-value">
                    <FormGroup legendText="Value">
                      <TextInput
                        id={"conditionValue" + index}
                        invalidText="A valid value is required"
                        //labelText="Value"
                        key={value.value + index}
                        placeholder="Value"
                        onBlur={(e) => this.changeConditionValue(e, index)}
                        size="sm"
                        defaultValue={value.value}
                      />
                    </FormGroup>
                  </div>
                  <div className="bx--col-lg-3">
                    <Dropdown
                      ariaLabel="Dropdown"
                      id={"conditionJoin" + index}
                      items={this.state.ddAndOrBlank}
                      label="--Please Select--"
                      titleText="AND/OR/Blank"
                      onChange={(e) => this.changeConditionJoin(e, index)}
                      size="sm"
                      selectedItem={value.joinOperator}
                    />
                  </div>
                </div>
              );
            })}
            {/* <div style={{height:"200px"}}>
        </div> */}
          </div>

          {this.uiButtonAddCondition()}
        </div>
      );
    } else {
      return;
    }
  };

  uiButtonAddCondition = () => {
    if (this.state.arrayConditions.length > 0) {
      return (
        <div style={{ overflow: "hidden" }}>
          <div style={{ float: "right" }}>
            <Button
              small
              disabled={
                this.state.arrayConditions[
                  this.state.arrayConditions.length - 1
                ].joinOperator !== "Blank" ||
                this.state.arrayConditions[
                  this.state.arrayConditions.length - 1
                ].field === "" ||
                this.state.arrayConditions[
                  this.state.arrayConditions.length - 1
                ].operator === "" ||
                this.state.arrayConditions[
                  this.state.arrayConditions.length - 1
                ].value === ""
              }
              onClick={this.editConditionsToGroup}
            >
              Submit
            </Button>
          </div>
        </div>
      );
    } else {
      return null;
    }
  };

  editConditionsToGroup = () => {
    console.log("this.state.arrayConditions", this.state.arrayConditions);
    this.editDTRows([...this.state.arrayConditions]);
    this.setState(
      {
        arrayConditions: [],
      },
      () => {
        console.log("this.state.arrayConditions", this.state.arrayConditions);
      }
    );
  };

  editDTRows = (tempcondition) => {
    var tempRowConditions = "";
    tempcondition.forEach((condition) => {
      tempRowConditions =
        tempRowConditions +
        condition.fieldName +
        " " +
        condition.operator +
        " " +
        condition.value +
        " ";
      if (condition.joinOperator !== "Blank") {
        tempRowConditions = tempRowConditions + condition.joinOperator + "";
      }
    });
    var tempConditionGroup = {
      conditions: tempcondition,
      fields: this.state.conditionsJson[this.state.editConditionIndex][
        "fields"
      ],
    };

    var tempConditionsJson = [...this.state.conditionsJson];
    tempConditionsJson[this.state.editConditionIndex] = tempConditionGroup;
    var tempRowData3 = [...this.state.rowData3];
    var tempRow = {
      id: tempRowData3[this.state.editConditionIndex]["id"],
      fields: tempRowData3[this.state.editConditionIndex]["fields"],
      conditions: tempRowConditions,
    };
    console.log("tempRow", tempRow);

    tempRowData3[this.state.editConditionIndex] = tempRow;

    this.setState(
      {
        //resetting it to blank, so it can get the updated grid
        rowData3: [],
        conditionsJson: tempConditionsJson,
        idRowData3: this.state.idRowData3 + 1,
      },
      () => {
        this.setState(
          {
            rowData3: tempRowData3,
          },
          () => {
            console.log("this.state.rowData3 after", this.state.rowData3);
            console.log("this.state.conditionsJson", this.state.conditionsJson);
          }
        );
      }
    );
  };

  AddToDTRows = () => {
    //initially conditions will be empty
    var tempRowConditions = "";

    var tempRowFields = "";
    var fieldArray = [...this.state.selectedFields];
    fieldArray.forEach((fieldItem) => {
      if (this.state.changedFieldsLabelsMap.has(fieldItem.field.value)) {
        fieldItem.field.key = this.state.changedFieldsLabelsMap.get(
          fieldItem.field.value
        );
      }
      tempRowFields = tempRowFields + fieldItem.field.key + ",";
    });

    var tempConditionGroup = {
      conditions: [],
      fields: fieldArray,
    };
    this.state.conditionsJson.push(tempConditionGroup);
    var tempRow = {
      id: this.state.idRowData3.toString(),
      fields: tempRowFields,
      conditions: tempRowConditions,
    };
    console.log("this.state.conditionsJson", this.state.conditionsJson);
    console.log("tempRow", tempRow);

    this.setState(
      {
        rowData3: [...this.state.rowData3, tempRow],
        idRowData3: this.state.idRowData3 + 1,
      },
      () => {
        this.validate();
      }
    );
  };

  uiDataTable2 = () => {
    if (this.state.rowData3.length > 0) {
      return (
        <div>
          <DataTable
            filterRows={function noRefCheck() {}}
            // rows={this.state.rowData2}
            rows={this.state.rowData3}
            headers={this.state.headerData3}
            size="compact"
            radio
            render={({
              rows,
              headers,
              getHeaderProps,
              getSelectionProps,
              getBatchActionProps,
              onInputChange,
              selectedRows,
            }) => (
              <TableContainer title="">
                {/* <TableToolbar>
                  make sure to apply getBatchActionProps so that the bar renders
                  <TableBatchActions {...getBatchActionProps()}>
                    <TableBatchAction
                      onClick={() => this.clickedEditRow(selectedRows)}
                    >
                      Edit
                    </TableBatchAction>
                    <TableBatchAction
                      onClick={() => this.clickedRemoveRow(selectedRows)}
                    >
                      Remove
                    </TableBatchAction>
                  </TableBatchActions>
                  <TableToolbarSearch onChange={onInputChange} />
                </TableToolbar> */}
                <Table>
                  <TableHead>
                    <TableRow>
                      {/* <TableSelectAll {...getSelectionProps()} />
                      <TableHeader>Select</TableHeader> */}
                      {headers.map((header) => (
                        <TableHeader
                          key={header.key}
                          {...getHeaderProps({ header })}
                        >
                          {header.header}
                        </TableHeader>
                      ))}
                      <TableHeader>Actions</TableHeader>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {rows.map((row, index) => (
                      <TableRow key={row.id}>
                        {/* <TableSelectRow {...getSelectionProps({ row })} />
                        <TableCell>check</TableCell> */}
                        {row.cells.map((cell) => (
                          <TableCell key={cell.id}>
                            {cell.value
                              ? cell.value
                              : "Click on 'Edit' Icon to add condition(s)"}
                          </TableCell>
                        ))}
                        <TableCell>
                          <div
                            title="Edit Group Conditions"
                            style={{ display: "inline" }}
                          >
                            <Edit32 onClick={() => this.editCondition(index)} />
                          </div>
                          {!this.isDeleteEnabled() && (
                            <div
                              title="Delete Group"
                              style={{ display: "inline" }}
                            >
                              <TrashCan32
                                onClick={() => {
                                  this.deleteCondition(index);
                                }}
                              />
                            </div>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                    {/* <TableRow>
                      <TableCell></TableCell>
                      <TableCell> {this.uiConditionBox()}</TableCell>
                      <TableCell></TableCell>
                    </TableRow> */}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
            locale="en"
            overflowMenuOnHover
            sortRow={function noRefCheck() {}}
            translateWithId={function noRefCheck() {}}
          />
        </div>
      );
    } else {
      return <div></div>;
    }
  };
  editCondition = (e) => {
    console.log("e", e);
    var index = e;
    console.log("index", index);
    var tempArrayCondition = [...this.state.conditionsJson[index].conditions];
    if (tempArrayCondition.length < 1) {
      tempArrayCondition.push(this.addInitialRow(index));
    }
    this.setState(
      {
        editConditionIndex: index,
        arrayConditions: [...tempArrayCondition],
      },
      () => {
        this.validate();
      }
    );
  };

  addInitialRow = (index) => {
    console.log("addInitialRow");
    var fields = [];
    if (this.state.selectedLevel === "Field") {
      fields = this.state.rbFields;
    } else {
      fields = this.state.cbFields;
    }
    var objRow = {
      id: this.state.rowId,
      field: fields[0].id,
      fieldName: fields[0].name,
      operator: "",
      value: "",
      joinOperator: "",
    };

    console.log("objRow", objRow);
    this.setState({
      rowId: this.state.rowId + 1,
    });
    return objRow;
  };

  isDeleteEnabled = () => {
    return this.state.arrayConditions.length > 0;
  };

  isFieldRbDisabled = () => {
    return this.state.rowData3.length === 1;
  };

  deleteCondition = (index) => {
    console.log("index", index);
    this.deleteRow(index);
    var tempConditionsjson = this.state.conditionsJson;
    tempConditionsjson.splice(index, 1);
    this.setState(
      {
        conditionsJson: [...tempConditionsjson],
      },
      () => {
        this.validate();
        console.log(
          "this.state.conditionsJson after delete",
          this.state.conditionsJson
        );
      }
    );
  };

  deleteRow = (index) => {
    console.log("rowData3", this.state.rowData3);
    var tempRowData3 = [...this.state.rowData3];
    tempRowData3.splice(index, 1);
    console.log("tempRowData3", tempRowData3);
    this.setState(
      {
        rowData3: [...tempRowData3],
      },
      () => {
        this.validate();
        console.log("rowData3", this.state.rowData3);
      }
    );
  };

  testmodalSaveClicked = () => {
    console.log("modalSaveClicked");
    if (1) {
      console.log("rowData", this.state.rowData2);
      var tempTableRowObj = {
        group: this.state.selectedModalGroup,
        id: this.state.placeholderId,
        field: this.state.selectedModalField,
        operator: this.state.selectedModalOperator,
        value: this.state.selectedModalValue,
        joinOperator: this.state.selectedModalAndOrBlank,
      };
      console.log("tempTableRowObj", tempTableRowObj);
      //   tempTableRowData.push(tempTableRowObj);
      //   console.log(tempTableRowData);
      this.setState(
        {
          rowData2: [...this.state.rowData2, tempTableRowObj],
          placeholderId: this.state.placeholderId + 1,
        },
        () => {
          console.log("rowData", this.state.rowData2);
        }
      );
    }
  };
  resetFieldTab = () => {
    this.setState({
      cbFields: [],
      rbFields: [],
      conditionsJson: [],
      editConditionIndex: -1,
      selectedFieldIdsArray: [],
      currentSelectedPageForFields: "",
      currentSelectedDocInFieldTab: "",
      selectedFields: [],
      rowData3: [],
      rowId: 200,
      idRowData3: 400,
      arrayConditions: [],
      changedFieldsLabelsMap: new Map(),
    });
  };

  resetPageTab = () => {
    this.setState({
      selectedPage: "",
      selectedPagesForDocLevel: [],
      selectedDocPageMap: new Map(),
      selectedPagesArray: [],
      //rbPages:[],
    });
  };

  resetDocumentTab = () => {
    console.log("inside reset");
    this.setState({
      selectedDocumentType: "",
      selectedDocumentsForBatchLevel: [],
      //rbDocumentTypes:[],
    });
  };
}
export default SampleReactFeature;
