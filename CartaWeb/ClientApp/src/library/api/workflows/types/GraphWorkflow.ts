import { WorkflowApi } from "library/api";
import Action from "./actors/actions";
import Selector, {
  SelectorExclude,
  SelectorInclude,
} from "./selectors/selectors";

export type GraphWorkflowEvent =
  | "selectorChanged"
  | "workflowChanged"
  | "workflowCreated";
type GraphWorkflowUserOperation = "selector" | "action";

interface SelectorAugment {
  include: string[];
  exclude: string[];
  inverted: boolean;
}

export default class GraphWorkflow {
  _id?: string;
  _selector: Selector;
  _selectorAugment: SelectorAugment;
  _callback?: () => Promise<any>;

  _userOperations: {
    operation: GraphWorkflowUserOperation;
    action?: Action;
    selector: Selector;
    selectorAugment: SelectorAugment;
  }[];
  _userOperationIndex: number;

  _eventHandlers: Record<GraphWorkflowEvent, Set<() => void>>;

  constructor(id?: string, callback?: () => Promise<any>) {
    this._id = id;
    this._selector = { type: "none" };
    this._selectorAugment = { include: [], exclude: [], inverted: false };

    this._userOperations = [
      {
        operation: "selector",
        selector: { type: "none" },
        selectorAugment: { include: [], exclude: [], inverted: false },
      },
    ];
    this._userOperationIndex = 0;

    this._eventHandlers = {
      selectorChanged: new Set(),
      workflowChanged: new Set(),
      workflowCreated: new Set(),
    };
    this._callback = callback;
  }

  async _create() {
    if (this._id !== undefined) return;
    else {
      // This will create the resource on the server and in turn give us the identifier.
      let props: any = {};
      if (this._callback) {
        props = await this._callback();
        if (props === null) return;
      }
      const workflowResource = await WorkflowApi.createWorkflowAsync({
        workflow: {
          name: "Unnamed Workflow",
          operations: [],
          ...props,
        },
      });
      this._id = workflowResource.id;
      this._callEvent("workflowCreated");
    }
  }
  async _appendOperation(operation: any) {
    if (this._id === undefined) return;
    await WorkflowApi.insertWorkflowOperationAsync({
      operation,
      workflowId: this._id.toString(),
    });
  }
  async _removeOperation() {
    if (this._id === undefined) return;
    await WorkflowApi.removeWorkflowOperationAsync({
      workflowId: this._id.toString(),
    });
  }

  _callEvent(type: GraphWorkflowEvent) {
    this._eventHandlers[type].forEach((handler) => handler());
  }

  on(type: GraphWorkflowEvent, handler: () => void) {
    this._eventHandlers[type].add(handler);
  }
  off(type: GraphWorkflowEvent, handler: () => void) {
    this._eventHandlers[type].delete(handler);
  }

  undo(remove?: boolean) {
    if (this._userOperationIndex > 0) {
      const userOperation = this._userOperations[this._userOperationIndex];
      if (userOperation.operation === "selector") {
        this._selector = userOperation.selector;
        this._selectorAugment = {
          include: [...userOperation.selectorAugment.include],
          exclude: [...userOperation.selectorAugment.exclude],
          inverted: userOperation.selectorAugment.inverted,
        };
        this._callEvent("selectorChanged");
      }
      if (userOperation.operation === "action" && userOperation.action) {
        this._selector = userOperation.selector;
        this._selectorAugment = {
          include: [...userOperation.selectorAugment.include],
          exclude: [...userOperation.selectorAugment.exclude],
          inverted: userOperation.selectorAugment.inverted,
        };
        this._removeOperation().finally(() => {
          this._callEvent("workflowChanged");
        });
      }
      this._userOperationIndex--;
      if (remove) this._userOperations.splice(this._userOperationIndex + 1);
    }
  }
  redo() {
    if (this._userOperationIndex + 1 < this._userOperations.length) {
      const userOperation = this._userOperations[this._userOperationIndex + 1];
      if (userOperation.operation === "selector") {
        this._selector = userOperation.selector;
        this._selectorAugment = {
          include: [...userOperation.selectorAugment.include],
          exclude: [...userOperation.selectorAugment.exclude],
          inverted: userOperation.selectorAugment.inverted,
        };
        this._callEvent("selectorChanged");
      }
      if (userOperation.operation === "action" && userOperation.action) {
        this._selector = userOperation.selector;
        this._selectorAugment = {
          include: [...userOperation.selectorAugment.include],
          exclude: [...userOperation.selectorAugment.exclude],
          inverted: userOperation.selectorAugment.inverted,
        };
        this._appendOperation({
          actor: userOperation.action,
          selector: this.getSelector(),
        }).finally(() => this._callEvent("workflowChanged"));
      }
      this._userOperationIndex++;
    }
  }

  recordOperation(operation: {
    operation: GraphWorkflowUserOperation;
    action?: Action;
    selector: Selector;
    selectorAugment: SelectorAugment;
  }) {
    const operations = this._userOperations
      .slice(0, this._userOperationIndex + 1)
      .concat([operation]);
    this._userOperations = operations;
    this._userOperationIndex++;
  }
  recordSelectionOperation() {
    this.recordOperation({
      operation: "selector",
      selector: { ...this._selector },
      selectorAugment: {
        include: [...this._selectorAugment.include],
        exclude: [...this._selectorAugment.exclude],
        inverted: this._selectorAugment.inverted,
      },
    });
  }
  recordActionOperation(action: Action) {
    this.recordOperation({
      operation: "action",
      action: action,
      selector: { ...this._selector },
      selectorAugment: {
        include: [...this._selectorAugment.include],
        exclude: [...this._selectorAugment.exclude],
        inverted: this._selectorAugment.inverted,
      },
    });
  }

  getSelector(): Selector {
    let selector = this._selector;
    if (this._selectorAugment.inverted) {
      selector = {
        type: "not",
        selector: selector,
      };
    }
    if (this._selectorAugment.include.length > 0) {
      const includeSelector: SelectorInclude = {
        type: "include",
        ids: [...this._selectorAugment.include],
      };
      selector = {
        type: "or",
        selectors: [includeSelector, selector],
      };
    }
    if (this._selectorAugment.exclude.length > 0) {
      const excludeSelector: SelectorExclude = {
        type: "exclude",
        ids: [...this._selectorAugment.exclude],
      };
      selector = {
        type: "and",
        selectors: [excludeSelector, selector],
      };
    }
    return selector;
  }
  applySelector(selector: Selector) {
    this._selector = selector;
    this._selectorAugment = {
      include: [],
      exclude: [],
      inverted: false,
    };
    this.recordSelectionOperation();
    this._callEvent("selectorChanged");
  }
  invertSelector() {
    const exclude = [...this._selectorAugment.exclude];
    const include = [...this._selectorAugment.include];
    this._selectorAugment.include = exclude;
    this._selectorAugment.exclude = include;
    this._selectorAugment.inverted = !this._selectorAugment.inverted;
    this.recordSelectionOperation();
    this._callEvent("selectorChanged");
  }
  addSelectorNodes(ids: string[]) {
    ids.forEach((id) => {
      const excludeIndex = this._selectorAugment.exclude.indexOf(id);
      if (excludeIndex > -1) {
        this._selectorAugment.exclude = [...this._selectorAugment.exclude];
        this._selectorAugment.exclude.splice(excludeIndex, 1);
      } else {
        this._selectorAugment.include = [...this._selectorAugment.include];
        this._selectorAugment.include.push(id);
      }
    });
    this.recordSelectionOperation();
    this._callEvent("selectorChanged");
  }
  removeSelectorNodes(ids: string[]) {
    ids.forEach((id) => {
      const includeIndex = this._selectorAugment.include.indexOf(id);
      if (includeIndex > -1) {
        this._selectorAugment.include = [...this._selectorAugment.include];
        this._selectorAugment.include.splice(includeIndex, 1);
      } else {
        this._selectorAugment.exclude = [...this._selectorAugment.exclude];
        this._selectorAugment.exclude.push(id);
      }
    });
    this.recordSelectionOperation();
    this._callEvent("selectorChanged");
  }

  applyAction(action: Action) {
    this._create()
      .then(() =>
        this._appendOperation({
          actor: action,
          selector: this.getSelector(),
        })
      )
      .then(
        () => {
          this.recordActionOperation(action);
          this._callEvent("workflowChanged");
        },
        () => {}
      );
  }
}
