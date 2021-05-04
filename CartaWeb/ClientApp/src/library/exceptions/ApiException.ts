class ApiException {
  type: string;
  url: string;
  status: number;
  message?: string;

  constructor(response: Response, message?: string) {
    this.type = ApiException.name;
    this.url = response.url;
    this.status = response.status;
    this.message = message;
  }
}

export default ApiException;
