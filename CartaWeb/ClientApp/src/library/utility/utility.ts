/**
 * Escapes a regular expression pattern. If surrounded by forward slashes '/' the pattern is assumed to already be in
 * regular expression format and is just trimmed. Otherwise, all regular expression characters are escaped and the
 * pattern is turned into an inclusion pattern.
 * @param pattern The pattern to escape.
 * @returns The regular expression escaped pattern.
 */
export function escapeRegex(pattern: string) {
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
    let patternReplace = pattern;
    escapeChars.forEach((char) => {
      patternReplace = patternReplace.replace(char, `\\${char}`);
    });
    return patternReplace;
  }
}
