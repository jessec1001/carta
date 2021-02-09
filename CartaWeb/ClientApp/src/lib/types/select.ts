import { Id } from "vis-data/declarations/data-interface";

export interface Selection {
    finite: Array<Id>,
    infinite: Array<Id>
}