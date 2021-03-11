import queryString from "query-string";

export * from "./api/data";
export * from "./api/meta";

export async function generalRequest(path: string, parameters?: Record<string, any>) {
    const url: string = queryString.stringifyUrl({
        url: path,
        query: parameters
    });

    const response = await fetch(url);
    return await response.json();
}