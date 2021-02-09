import { Id } from "vis-data/declarations/data-interface";

export interface Selection {
    /** The IDs of nodes that are selected. */
    shallow: Array<Id>,
    /** The IDs of nodes that have hidden selected children. */
    deep: Array<Id>
}