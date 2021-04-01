export function replaceRegex(pattern: string) {
  const escapeChars = [
    "\\",
    "[",
    "]",
    ".",
    "*",
    "{",
    "}",
    "(",
    ")",
    "?",
    "+",
    "|",
  ];
  const existingRegex =
    pattern.length >= 2 &&
    pattern[0] === "/" &&
    pattern[pattern.length - 1] === "/";

  if (existingRegex) {
    return pattern.substring(1, pattern.length - 1);
  } else {
  }
}
