type AxiosErrorLike = {
  response?: {
    data?: {
      message?: unknown;
    };
  };
};

function isAxiosError(error: unknown): error is { response: { data: { message: string } } } {
  return (
    typeof error === 'object' &&
    error !== null &&
    'response' in error &&
    typeof (error as AxiosErrorLike).response?.data?.message === 'string'
  );
}