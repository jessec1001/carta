import queryString from "query-string";

import { ApiCollection } from "../types/meta";

/** Gets the Carta API endpoints. */
export async function metaGetEndpoints() {
  const url: string = queryString.stringifyUrl({
    url: `api/meta`,
  });

  const response = await fetch(url);
  const data = await response.json();

  return data as ApiCollection[];
}
