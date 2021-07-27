/**
 * Converts a string to capital case.
 * That is, the first letter in the string is uppercase and the rest of the string is lowercase.
 * @param text The text to convert.
 * @returns The converted case.
 */
export function toCapitalCase(text: string) {
  return text[0].toUpperCase() + text.substring(1).toLowerCase();
}

/**
 * Converts a string to title case.
 * That is, the first letter in each word in the string is uppercase and the rest of the string is lowercase.
 * @param text
 * @returns
 */
export function toTitleCase(text: string) {
  return text.split(" ").map(toCapitalCase).join(" ");
}
